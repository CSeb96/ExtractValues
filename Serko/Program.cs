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
 * */
namespace Serko
{
    class Program
    {
        // Variables used in this program.
        #region Variables
        private static readonly HttpClient client = new HttpClient();
        // Assumption: urlForPost is the url for the web service. 
        private static string urlForPost = "Url://urladdress.com/url";
        private static string FILENAME = "C:\\Users\\Cyril Sebastian\\Desktop\\email.txt";
        private static StreamReader reader;
        private static string txtLines = " ";
        private static string txtXml = " ";
        private static double totalMoney = 0;
        private static readonly double gst = 0.15;
        private static double excludesGST = 0;
        private static Stack<char> bracketStack = new Stack<char>();
        #endregion
        static void Main(string[] args)
        {

            reader = new StreamReader(FILENAME);
            Program pg = new Program();
            // Getting the response from the post method.
            try
            {
                var response = pg.GetTaxAsync();
                if (response.IsCompleted)
                {
                    // Success
                }
            }
            catch (Exception error)
            {
                // Return error message with error code
            }
        }

        public async Task<string> GetTaxAsync()
        {

            // Console.WriteLine(CheckForMissingTags());


            while ((txtLines = reader.ReadLine()) != null)
            {

                if (txtLines.StartsWith("<") && txtLines.EndsWith(">"))
                {
                    txtXml += txtLines + "\n";

                }
                else
                {
                    string regexPattern = @"(<.*>)(.*)(<\/.*>)";
                    Regex regex = new Regex(regexPattern, RegexOptions.Singleline);

                    //txtXml += regex.Matches(txtLines);
                    MatchCollection collection = regex.Matches(txtLines);

                    var list = collection.Cast<Match>().Select(match => match.Value).ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        txtXml += list.ElementAt(i);
                    }
                }
            }

           XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml("<root>" + txtXml + "</root>");
            }
            catch(Exception err)
            {
                // Will catch if there is any tags that aren't closed.
            }
        
            // Get the total from the newly formed XML document.

            XmlNode tNode = doc.GetElementsByTagName("total")[0];
            XmlNode costCentre = doc.GetElementsByTagName("cost_centre")[0];
            XmlNode paymentMethod = doc.GetElementsByTagName("payment_method")[0];
            XmlNode vendor = doc.GetElementsByTagName("vendor")[0];
            XmlNode description = doc.GetElementsByTagName("description")[0];
            XmlNode date = doc.GetElementsByTagName("date")[0];

            if (tNode == null)
            {
                return "Total tag wasn't present";
            }
            else
            {
                totalMoney = Convert.ToDouble(tNode.InnerXml);
                excludesGST = totalMoney - (totalMoney * gst);


                // Code to post the gst information back to the web service.

                var xmlValues = new Dictionary<string, string>
                {
                    {"costCentre", costCentre.InnerXml },
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
    }
}


