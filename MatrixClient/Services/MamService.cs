using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using XmppDotNet;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.Delay;
using XmppDotNet.Xmpp.Forward;
using XmppDotNet.Xmpp.MessageArchiveManagement;
using XmppDotNet.Xmpp.ResultSetManagement;
using XmppDotNet.Xmpp.XData;

namespace MatrixClient.Services
{
  internal class MamService
  {

    /// <summary>
    /// Task to retrieve messages for a given Jid from archive
    /// </summary>
    /// <param name="xmppClient">XmppClient instance</param>
    /// <param name="jid">The jid we request messages fore</param>
    /// <param name="maxResults">Max number results (paging)</param>
    /// <returns>MamResult object</returns>
    public async Task<MamResult> RequestLastChatMessagesFromArchive(
        XmppClient xmppClient,
        string jid,
        int maxResults
        )
    {
      // build the MAM query
      var mamQuery = new IqQuery<MessageArchive>
      {
        Type = IqType.Set,
        Query =
        {
            QueryId = Guid.NewGuid().ToString(),
            XData = new Data
            {
                Type = FormType.Submit,
                Fields = new[]
                {
                    new Field
                    {
                        Var = "FORM_TYPE",
                        Type = FieldType.Hidden,
                        Values = new[] {Namespaces.MessageArchiveManagement}
                    },
                    new Field
                    {
                        Var = "with",
                        Values = new[] {jid}
                    }
                }
            },
            ResultSet = new Set
            {
                Max = maxResults
            }
        }
      };

      var messages = new List<Forwarded>();

      // set up message subscription
      // we look for messages that:
      // * match our query id
      // * are a MAM result messages
      // * are sent from us or the Jid we query for
      var messageSubscription = xmppClient
         .XmppXElementReceived
         .Where(el => el is Message { IsMamResult: true } msg &&
                      msg.MamResult.QueryId == mamQuery.Query.QueryId &&
                      (msg.From.EqualsBare(jid) || msg.From.EqualsBare(xmppClient.Jid)))
         .Select(el => el.Cast<Message>().MamResult)
         .Subscribe(result =>
         {
           var forwardedMessage = result.Forwarded;
           messages.Add(forwardedMessage);
         });


      var resIq = await xmppClient.SendIqAsync(mamQuery);

      // dispose the subscription
      messageSubscription.Dispose();

      // return iq result and messages in the MamResult object
      return MamResult
          .FromIq(resIq)
          .WithMessages(messages);
    }
  }
  public class MamResult
  {
    // was this request a Success or failure
    public bool IsSuccess { get; set; }
    // is the result complete, or can we request more pages
    public bool Complete { get; set; }
    // last is in the results for paging
    public string Last { get; set; }
    // first is in teh results for paging
    public string First { get; set; }

    // collection of messages retrieved
    public ReadOnlyCollection<Forwarded> Messages { get; set; }

    public MamResult WithMessages(List<Forwarded> messages)
    {
      Messages = new ReadOnlyCollection<Forwarded>(messages);
      return this;
    }

    public static MamResult FromIq(Iq iq)
    {
      if (iq.Type == IqType.Result)
      {
        // success
        if (iq.Query is Final final)
        {
          return new MamResult()
          {
            IsSuccess = true,
            Complete = final.Complete,
            Last = final.ResultSet.Last,
            First = final.ResultSet.First.Value
          };
        }
      }

      return new MamResult()
      {
        IsSuccess = false,
      };
    }
  }
}
