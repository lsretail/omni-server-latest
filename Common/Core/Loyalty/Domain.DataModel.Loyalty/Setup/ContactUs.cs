using System;
using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    public class ContactUs
    {
        public class ContactLine
        {
            public enum ContactType
            {
                Web = 0,
                Phone = 1,
                Email = 2
            }

            public ContactType Type { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        public List<string> HeaderLines { get; set; }
        public List<ContactLine> ContactLines { get; set; }
        public List<string> FooterLines { get; set; }

        public ContactUs(string htmlText)
        {
            HeaderLines = new List<string>();
            ContactLines = new List<ContactLine>();
            FooterLines = new List<string>();

            htmlText = htmlText.Trim().Replace("\r\n", "");
            htmlText = htmlText.Trim().Replace("\n\r", "");

            //HEADER LINES
            while (htmlText.StartsWith("<p>"))
            {
                string headerLine = htmlText.Substring(0, htmlText.IndexOf("</p>") + 4);
                HeaderLines.Add(headerLine.Substring(3, headerLine.Length - 7));
                htmlText = htmlText.Remove(0, headerLine.Length);
            }

            //HEADER LINES
            while (htmlText.TrimStart(' ').StartsWith("<a"))
            {
                ContactLine contactLine = new ContactLine();
                string line = htmlText.Substring(0, htmlText.IndexOf("</a>") + 4).TrimStart(' ');
                htmlText = htmlText.Remove(0, htmlText.IndexOf("</a>") + 4);

                line = line.Remove(0, 9);

                if (line.StartsWith("tel"))
                {
                    contactLine.Type = ContactLine.ContactType.Phone;
                    line = line.Remove(0, 4);
                    if (line.StartsWith("+"))
                        line = line.Remove(0, 1);

                    int number;
                    while (int.TryParse(line[0].ToString(), out number))
                    {
                        contactLine.Value += number.ToString();
                        line = line.Remove(0, 1);
                    }

                    line = line.Remove(0, 2);
                    contactLine.Description = line.Substring(0, line.IndexOf("</a>"));
                }
                else if (line.StartsWith("mail"))
                {
                    contactLine.Type = ContactLine.ContactType.Email;
                    line = line.Remove(0, 5);

                    while (line[0] != '"')
                    {
                        contactLine.Value += line[0];
                        line = line.Remove(0, 1);
                    }

                    line = line.Remove(0, 2);
                    contactLine.Description = line.Substring(0, line.IndexOf("</a>"));
                }
                else if (line.StartsWith("url"))
                {
                    contactLine.Type = ContactLine.ContactType.Web;
                    line = line.Remove(0, 4);

                    while (line[0] != '"')
                    {
                        contactLine.Value += line[0];
                        line = line.Remove(0, 1);
                    }

                    line = line.Remove(0, 2);
                    contactLine.Description = line.Substring(0, line.IndexOf("</a>"));
                }

                ContactLines.Add(contactLine);
            }

            //HEADER LINES
            while (htmlText.StartsWith("<p>"))
            {
                string footerLine = htmlText.Substring(0, htmlText.IndexOf("</p>") + 4);
                FooterLines.Add(footerLine.Substring(3, footerLine.Length - 7));
                htmlText = htmlText.Remove(0, footerLine.Length);
            }
        }
    }
}
