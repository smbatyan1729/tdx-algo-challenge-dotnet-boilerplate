using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using NodaTime.Text;

namespace Challenge
{
    public class InstantTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => InstantPattern.General.Parse(text).Value;
    }
}