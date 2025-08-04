using System;
using System.Globalization;
using System.Xml.Linq;

namespace XmppDotNet.Xmpp.Rpc
{
    internal class XmlRpcWriter
    {
        public static XElement WriteParams(Parameters Params)
        {
            if (Params != null && Params.Count > 0)
            {
                if (Params[0] is XmlRpcException)
                    return WriteFault(Params[0] as XmlRpcException);
                
                var elParams = new Params();
                foreach (object t in Params)
                {
                    var param = new Param();
                    WriteValue(t, param);
                    elParams.Add(param);
                }
                return elParams;
            }
            return null;
        }

        private static Fault WriteFault(XmlRpcException ex)
        {
            var fault = new Fault();
            WriteValue(
                new StructParameter
                {
                    {"faultCode", ex.Code},
                    {"faultString", ex.Message}
                }
                , fault);

            return fault;
        }
        /// <summary>
        /// Writes a single value to a call
        /// </summary>
        /// <param name="param"></param>
        /// <param name="parent"></param>
        private static void WriteValue(object param, XElement parent)
        {
            var value = new Value();

            if (param is String)
            {
                value.AddTag("string", param as string);
            }
            else if (param is Int32)
            {
                value.AddTag("i4", ((Int32)param).ToString());
            }
            else if (param is Int16) // code in fault
            {
                value.AddTag("int", ((Int16)param).ToString());
            }
            else if (param is Double)
            {
                var numberInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };
                value.AddTag("double", ((Double)param).ToString(numberInfo));
            }
            else if (param is Boolean)
            {
                value.AddTag("boolean", ((bool)param) ? "1" : "0");
            }
            // XML-RPC dates are formatted in iso8601 standard, same as xmpp,
            else if (param is DateTime)
            {
                value.AddTag("dateTime.iso8601", XmppDotNet.Time.JabberDate((DateTime)param));
            }
            // byte arrays must be encoded in Base64 encoding
            else if (param is byte[])
            {
                var b = (byte[])param;
                value.AddTag("base64", Convert.ToBase64String(b, 0, b.Length));
            }
            // Arraylist maps to an XML-RPC array
            else if (param is Parameters)
            {
                //<array>  
                //    <data>
                //        <value>  <string>one</string>  </value>
                //        <value>  <string>two</string>  </value>
                //        <value>  <string>three</string></value>  
                //    </data> 
                //</array>
                var array = new Array();
                var data = new Data();

                var list = param as Parameters;

                foreach (object t in list)
                    WriteValue(t, data);
                

                array.Add(data);
                value.Add(array);
            }
            // jStructParameter maps to an XML-RPC struct
            else if (param is StructParameter)
            {
                var elStruct = new Struct();

                var structParameter = param as StructParameter;
                
                foreach (var entry in structParameter)
                {
                    var member = new Member();
                    
                    member.Add(new Name { Value = entry.Key });
                    WriteValue(entry.Value, member);

                    elStruct.Add(member);    
                }
                value.Add(elStruct);
            }
            /*
            else
            {
                // Unknown Type
            }
            */
            parent.Add(value);
        }
    }
}
