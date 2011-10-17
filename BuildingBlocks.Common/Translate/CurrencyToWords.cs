using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;

namespace BuildingBlocks.Common.Translate
{
    public class CurrencyToWords
    {
        private static HybridDictionary currencies = new HybridDictionary();

        static CurrencyToWords()
        {
            Register("RUR", true, "�����", "�����", "������", "�������", "�������", "������");
            Register("EUR", true, "����", "����", "����", "��������", "���������", "����������");
            Register("USD", true, "������", "�������", "��������", "����", "�����", "������");
            ConfigurationSettings.GetConfig("currency-names");
        }

        public static void Register(string currency, bool male,
            string seniorOne, string seniorTwo, string seniorFive,
            string juniorOne, string juniorTwo, string juniorFive)
        {
            CurrencyInfo info;
            info.male = male;
            info.seniorOne = seniorOne; info.seniorTwo = seniorTwo; info.seniorFive = seniorFive;
            info.juniorOne = juniorOne; info.juniorTwo = juniorTwo; info.juniorFive = juniorFive;
            currencies.Add(currency, info);
        }

        public static string ToWords(decimal val)
        {
            return ToWords(val, "RUR");
        }

        public static string ToWords(decimal val, string currency)
        {
            if (!currencies.Contains(currency))
                throw new ArgumentOutOfRangeException("currency", "������ \"" + currency + "\" �� ����������������");

            CurrencyInfo info = (CurrencyInfo)currencies[currency];
            return ToWords(val, info.male,
                info.seniorOne, info.seniorTwo, info.seniorFive,
                info.juniorOne, info.juniorTwo, info.juniorFive);
        }

        public static string ToWords(decimal val, bool male,
            string seniorOne, string seniorTwo, string seniorFive,
            string juniorOne, string juniorTwo, string juniorFive)
        {
            bool minus = false;
            if (val < 0) { val = -val; minus = true; }

            int n = (int)val;
            int remainder = (int) ((val - n + (decimal) 0.005) * 100);

            StringBuilder r = new StringBuilder();

            if (0 == n) r.Append("0 ");
            if (n % 1000 != 0)
                r.Append(RusNumber.Str(n, male, seniorOne, seniorTwo, seniorFive));
            else
                r.Append(seniorFive);

            n /= 1000;

            r.Insert(0, RusNumber.Str(n, false, "������", "������", "�����"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "�������", "��������", "���������"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "��������", "���������", "����������"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "��������", "���������", "����������"));
            n /= 1000;

            r.Insert(0, RusNumber.Str(n, true, "���������", "����������", "�����������"));
            if (minus) r.Insert(0, "����� ");

            r.Append(remainder.ToString("00 "));
            r.Append(RusNumber.GrammaticalNumberForValue(remainder, juniorOne, juniorTwo, juniorFive));

            //������ ������ ����� ���������
            r[0] = char.ToUpper(r[0]);

            return r.ToString();
        }
    };
    struct CurrencyInfo
    {
        public bool male;
        public string seniorOne, seniorTwo, seniorFive;
        public string juniorOne, juniorTwo, juniorFive;
    };
};