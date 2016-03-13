using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;


namespace Finanbot.Apis
{
    public class ExchangeOffice
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Rate { get; set; }
    }
    [Serializable, XmlRoot("table")]
    public class RatesMap
    {
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class ValCurs
        {
            [XmlElementAttribute("row")]
            public ValCursValute[] ValuteList;
        }

        public class ValCursValute
        {

            [XmlElementAttribute("office__lat")]
            public string Latitude;

            [XmlElementAttribute("office__lng")]
            public string Longitude;

            [XmlElementAttribute("rate__rate_out")]
            public string Rate;
        }

        private const float lt = 0.5f;
        private const float lg = 0.5f;
        public static List<ExchangeOffice> GetExchangeOffices(float latitude, float longitude, string from, string to)
        {
            var url = string.Format(CultureInfo.InvariantCulture,
                "http://4map.ru/service/get_rates_to_map.aspx?latsw={0}&lngsw={1}&latne={2}&lngne={3}&zoom=11&curr1={4}&curr2={5}",
                latitude - lt, longitude - lg, latitude + lt, longitude + lg, from, to);

            List<ExchangeOffice> result = new List<ExchangeOffice>();
            XmlReader xr = new XmlTextReader(url);
            var doc = new XmlDocument();
            var t = doc.ReadNode(xr);
            foreach (XmlNode node in t.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;

                var lat = node.SelectSingleNode("office__lat");

                //*
                result.Add(new ExchangeOffice()
                {
                    Latitude = float.Parse(node.SelectSingleNode("office__lat").InnerText, CultureInfo.InvariantCulture),
                    Longitude = float.Parse(node.SelectSingleNode("office__lng").InnerText, CultureInfo.InvariantCulture),
                    Rate = float.Parse(node.SelectSingleNode("rate__rate_out").InnerText, CultureInfo.InvariantCulture)
                });//*/
            }
            return result;
        }
    }
}
