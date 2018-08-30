using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Serko
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Variables
            string FILENAME = "C:\\Users\\Cyril Sebastian\\Desktop\\email.txt";
            StreamReader reader = new StreamReader(FILENAME);
            string lines = " ";
            string xml = " ";
            double totalM = 0;
            double gst = 0.15;
            double excludesGST = 0;
            Stack<char> bracketStack = new Stack<char>();
            #endregion

            while ((lines = reader.ReadLine()) != null)
            {
                if (lines.Contains('<'))
                {
                    bracketStack.Push('<');
                }else if(lines.Contains('>')){
                    bracketStack.Pop();
                }

                if(bracketStack.Count != 0)
                {
                    Console.WriteLine("Not valid text");
                }
                else
                {
                    if (lines.StartsWith("<") && lines.EndsWith(">"))
                        xml += lines + "\n";
                }
               
            }


            if (!xml.Contains("total"))
            {
                Console.WriteLine("Doesn't contain total tag");
            }
            else
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(xml);
                XmlNodeList nodeList = doc.GetElementsByTagName("expense");

                XmlNode tNode = doc.GetElementsByTagName("total")[0];

                totalM = Convert.ToDouble(tNode.InnerXml);
                excludesGST = totalM - (totalM * gst);
            }

        }
    }
}


