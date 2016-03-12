using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;
using System;

namespace Finanbot.Apis
{
    public class CurrencyRate
    {
        /// <summary>
        /// Закодированное строковое обозначение валюты
        /// Например: USD, EUR, AUD и т.д.
        /// </summary>
        public string CharCode { get; set; }

        /// <summary>
        /// Наименование валюты
        /// Например: Доллар, Евро и т.д.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Обменный курс
        /// </summary>
        public double Value { get; set; }
    }

    public class CurrencyRates
    {
        public class ValCurs
        {
            [XmlElementAttribute("Valute")]
            public ValCursValute[] ValuteList;
        }

        public class ValCursValute
        {

            [XmlElementAttribute("CharCode")]
            public string ValuteStringCode;

            [XmlElementAttribute("Name")]
            public string ValuteName;

            [XmlElementAttribute("Value")]
            public string ExchangeRate;
        }

        /// <summary>
        /// Получить список котировок ЦБ ФР на данный момент
        /// </summary>
        /// <returns>список котировок ЦБ РФ</returns>
        public static List<CurrencyRate> GetExchangeRates()
        {
            List<CurrencyRate> result = new List<CurrencyRate>();
            XmlSerializer xs = new XmlSerializer(typeof(ValCurs));
            XmlReader xr = new XmlTextReader(@"http://www.cbr.ru/scripts/XML_daily.asp");
            foreach (ValCursValute valute in ((ValCurs)xs.Deserialize(xr)).ValuteList)
            {
                result.Add(new CurrencyRate()
                {
                    CharCode = valute.ValuteStringCode,
                    Name = valute.ValuteName,
                    Value = Convert.ToDouble(valute.ExchangeRate)
                });
            }
            return result;
        }
    }
}