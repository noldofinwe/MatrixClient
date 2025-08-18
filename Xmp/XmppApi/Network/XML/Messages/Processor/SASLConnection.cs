﻿using System;
using System.Collections;
using System.Threading.Tasks;
using Logging;
using XmppApi.Network.Events;
using XmppApi.Network.TCP;
using XmppApi.Network.XML.Messages.Features;
using XmppApi.Network.XML.Messages.Features.SASL;
using XmppApi.Network.XML.Messages.Features.SASL.Plain;
using XmppApi.Network.XML.Messages.Features.SASL.SCRAM;
using XmppApi.Network.XML.Messages.Features.SASL.SCRAM.SHA1;
using XmppApi.Network.XML.Messages.Features.SASL.SCRAM.SHA256;

namespace XmppApi.Network.XML.Messages.Processor
{
    public class SASLConnection: AbstractMessageProcessor
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private SASLState state;
        private AbstractSASLMechanism selectedMechanism;
        // The offered authentication mechanism in preferred order:
        private static readonly ArrayList OFFERED_MECHANISMS = new ArrayList() { "scram-sha-256", "scram-sha-1", "plain" };

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 22/08/2017 Created [Fabian Sauter]
        /// </history>
        public SASLConnection(TcpConnection tcpConnection, XmppConnection xmppConnection) : base(tcpConnection, xmppConnection)
        {
            reset();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        private ArrayList getMechanisms(StreamFeaturesMessage msg)
        {
            foreach (StreamFeature sF in msg.getFeatures())
            {
                if (sF is SASLStreamFeature)
                {
                    return (sF as SASLStreamFeature).MECHANISMS;
                }
            }
            return null;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public override void reset()
        {
            state = SASLState.DISCONNECTED;
            selectedMechanism = null;
            startListeningForMessages();
        }

        #endregion

        #region --Misc Methods (Private)--
        private void selectMechanism(ArrayList mechanisms)
        {
            string selected = null;
            foreach (string s in OFFERED_MECHANISMS)
            {
                foreach (string m in mechanisms)
                {
                    if (m.ToLowerInvariant().Equals(s))
                    {
                        selected = s;
                        break;
                    }
                }
                if (selected != null)
                {
                    break;
                }
            }
            XMPPAccount sCC = XMPP_CONNECTION.account;
            switch (selected)
            {
                case "scram-sha-256":
                    selectedMechanism = new ScramSHA256SASLMechanism(sCC.user.localPart, sCC.user.password, this);
                    break;

                case "scram-sha-1":
                    selectedMechanism = new ScramSHA1SASLMechanism(sCC.user.localPart, sCC.user.password, this);
                    break;

                case "plain":
                    selectedMechanism = new PlainSASLMechanism(sCC.user.localPart, sCC.user.password, this);
                    break;

                default:
                    onSaslError("Failed to select authentication mechanism - \"" + selected + "\" is no supported mechanism!", SASLState.NO_VALID_MECHANISM);
                    break;
            }
        }

        public void onSaslError(string errMsg, SASLState newState)
        {
            Task.Run(async () =>
            {
                Logger.Error(errMsg);
                await XMPP_CONNECTION.OnMessageProcessorFailedAsync(new ConnectionError(ConnectionErrorCode.SASL_FAILED, errMsg), true);
                state = newState;
            });
        }

        #endregion

        #region --Misc Methods (Protected)--
        protected async override Task processMessageAsync(NewValidMessageEventArgs args)
        {
            AbstractMessage msg = args.MESSAGE;
            if (msg.isProcessed())
            {
                return;
            }

            switch (state)
            {
                case SASLState.DISCONNECTED:
                    if (msg is StreamFeaturesMessage || msg is OpenStreamAnswerMessage)
                    {
                        StreamFeaturesMessage features = null;
                        if (msg is OpenStreamAnswerMessage)
                        {
                            features = (msg as OpenStreamAnswerMessage).getStreamFeaturesMessage();
                        }
                        else
                        {
                            features = msg as StreamFeaturesMessage;
                        }

                        if (features is null)
                        {
                            return;
                        }

                        ArrayList mechanisms = getMechanisms(features);
                        if (mechanisms is null)
                        {
                            return;
                        }
                        setMessageProcessed(args);
                        selectMechanism(mechanisms);
                        if (selectedMechanism is null)
                        {
                            state = SASLState.ERROR;
                            await XMPP_CONNECTION.OnMessageProcessorFailedAsync(new ConnectionError(ConnectionErrorCode.SASL_FAILED, "selectedMechanism is null"), true);
                            return;
                        }
                        await XMPP_CONNECTION.SendAsync(selectedMechanism.getSelectSASLMechanismMessage(), true);
                        state = SASLState.REQUESTED;
                    }
                    break;

                case SASLState.REQUESTED:
                case SASLState.CHALLENGING:
                    if (msg is ScramSHAChallengeMessage)
                    {
                        state = SASLState.CHALLENGING;
                        setMessageProcessed(args);
                        AbstractMessage response = selectedMechanism.generateResponse(msg);
                        if (!(response is null))
                        {
                            await XMPP_CONNECTION.SendAsync(response, true);
                        }
                    }
                    else if (msg is SASLSuccessMessage)
                    {
                        state = SASLState.CONNECTED;
                        msg.setRestartConnection(AbstractMessage.SOFT_RESTART);
                        setMessageProcessed(args);
                        stopListeningForMessages();
                    }
                    else if (msg is SASLFailureMessage)
                    {
                        stopListeningForMessages();
                        SASLFailureMessage saslFailureMessage = msg as SASLFailureMessage;
                        state = SASLState.ERROR;

                        Logger.Error("Error during SASL authentication: " + saslFailureMessage.ERROR_TYPE + "\n" + saslFailureMessage.ERROR_MESSAGE);
                        if (saslFailureMessage.ERROR_TYPE == SASLErrorType.UNKNOWN_ERROR)
                        {
                            await XMPP_CONNECTION.OnMessageProcessorFailedAsync(new ConnectionError(ConnectionErrorCode.SASL_FAILED, "SASL: " + saslFailureMessage.ERROR_MESSAGE), true);
                        }
                        else
                        {
                            await XMPP_CONNECTION.OnMessageProcessorFailedAsync(new ConnectionError(ConnectionErrorCode.SASL_FAILED, "SASL: " + saslFailureMessage.ERROR_TYPE), true);
                        }
                    }
                    break;

                case SASLState.CONNECTED:
                    break;

                default:
                    throw new InvalidOperationException("Unexpected value for state: " + state);
            }
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
