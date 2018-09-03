using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

/*
 * Assumptions about this program.
 *  1) There is a web service online that sends the email text and can recieve the information.
 *  2) GST is 15%.
 *  3) The extracted values are sent to the client online in string form.
 *  4) The Xml attributes are always the same
 *  5) I added the text from the email to resources in this project so I can call Resources.Email for testing.
 * */
namespace Serko
{
    class Program
    {
        // Variables used in this program.
        #region Variables
        private static readonly HttpClient client = new HttpClient();
        private static string urlForPost = "URL FOR POST";
        private static string FILENAME = "LOCAL ADDRESS OF THE FILE";
        private static string FILE = Properties.Resources.Email;
        private static string txtXml = " ";
        private static double totalMoney = 0;
        private static readonly double gst = 0.15;
        private static double excludesGST = 0;
        private static Stack<char> bracketStack = new Stack<char>();
        #endregion
        static void Main(string[] args)
        {

            Program pg = new Program();
            // Getting the response from the post method.
            try
            {
                var response = pg.GetTaxAsync(FILE);
                if (response.IsCompleted)
                {
                    // Success
                }
            }
            catch (Exception error)
            {
                // Return error message with error code
            }
            finally
            {
                client.Dispose();
            }
        }

        public async Task<string> GetTaxAsync(string text)
        {

            StringReader reader = new StringReader(text);
            while ((text = reader.ReadLine()) != null)
            {
                if (text.StartsWith("<") && text.EndsWith(">"))
                {
                    txtXml += text + "\n";

                }
                else
                {
                    string regexPattern = @"(<.*>)(.*)(<\/.*>)";
                    Regex regex = new Regex(regexPattern, RegexOptions.Singleline);

                    MatchCollection collection = regex.Matches(text);

                    var list = collection.Cast<Match>().Select(match => match.Value).ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        txtXml += list.ElementAt(i);
                    }
                }
            }

            Console.Write(txtXml);
            XmlDocument doc = new XmlDocument();

            try
            {
                string cost_Centre = " ";
                doc.LoadXml("<root>" + txtXml + "</root>");

                #region Nodes
                XmlNode tNode = doc.GetElementsByTagName("total")[0];
                XmlNode costCentre = doc.GetElementsByTagName("cost_centre")[0];
                XmlNode paymentMethod = doc.GetElementsByTagName("payment_method")[0];
                XmlNode vendor = doc.GetElementsByTagName("vendor")[0];
                XmlNode description = doc.GetElementsByTagName("description")[0];
                XmlNode date = doc.GetElementsByTagName("date")[0];
                #endregion

                // If cost centre node isn't found, set up a string variable in place of it when sending the data
                // back to web service. 
                if (costCentre == null)
                {
                    cost_Centre = "UNKNOWN";
                }
                else
                {
                    cost_Centre = costCentre.InnerXml;
                }

                if (tNode == null)
                {
                    // Halt the program
                    return "No total tag present";

                }
                else
                {
                    totalMoney = Convert.ToDouble(tNode.InnerXml);
                    excludesGST = totalMoney - (totalMoney * gst);


                    // Code to post the gst information back to the web service.

                    var xmlValues = new Dictionary<string, string>
                {
                    {"costCentre", cost_Centre },
                    {"gst", "0.15" },
                    {"totalNoGst", excludesGST.ToString() },
                    {"paymentMethod", paymentMethod.InnerXml },
                    {"vendor", vendor.InnerXml },
                    {"description", description.InnerXml },
                    {"date", date.InnerXml }
                };

                    var values = new FormUrlEncodedContent(xmlValues);
                    var response = await client.PostAsync(urlForPost, values);
                    var responseString = await response.Content.ReadAsStringAsync();

                    return responseString;
                }

            }
            catch (Exception err)
            {
                // An error message will display information about any unclosed tags.
                // Will also catch if any nodes aren't present.
            }

            return "False";

        }
    }
}


