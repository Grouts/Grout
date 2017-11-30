using System;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Grout.Base.DataClasses;

namespace Grout.Base.Reports.Scheduler
{
    [Serializable]
    public class ScheduleHelper
    {
        public DaysOfWeek GetDaysofWeek(DayOfTheWeek days)
        {
            DaysOfWeek weekDays;
            switch (days)
            {

                case DayOfTheWeek.Monday:
                    weekDays = new DaysOfWeek { Monday = true };
                    break;
                case DayOfTheWeek.Tuesday:
                    weekDays = new DaysOfWeek { Tuesday = true };
                    break;
                case DayOfTheWeek.Wednesday:
                    weekDays = new DaysOfWeek { Wednesday = true };
                    break;
                case DayOfTheWeek.Thursday:
                    weekDays = new DaysOfWeek { Thursday = true };
                    break;
                case DayOfTheWeek.Friday:
                    weekDays = new DaysOfWeek { Friday = true };
                    break;
                case DayOfTheWeek.Saturday:
                    weekDays = new DaysOfWeek { Saturday = true };
                    break;
                case DayOfTheWeek.Sunday:
                    weekDays = new DaysOfWeek { Sunday = true };
                    break;
                case DayOfTheWeek.weekday:
                    weekDays = new DaysOfWeek { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true };
                    break;
                case DayOfTheWeek.weekendday:
                    weekDays = new DaysOfWeek { Saturday = true, Sunday = true };
                    break;
                default:
                    weekDays = new DaysOfWeek { Day = true };
                    break;
            }
            return weekDays;
        }

        public DayOfTheWeek GetWeekDays(DaysOfWeek days)
        {
            DayOfTheWeek dayofTheWeek;
            if (days.Monday == true && days.Tuesday == true && days.Wednesday == true && days.Thursday == true && days.Friday == true)
            {
                dayofTheWeek = DayOfTheWeek.weekday;
            }
            else if (days.Sunday == true && days.Saturday == true)
            {
                dayofTheWeek = DayOfTheWeek.weekendday;
            }
            else if (days.Monday == true)
            {
                dayofTheWeek = DayOfTheWeek.Monday;
            }
            else if (days.Tuesday == true)
            {
                dayofTheWeek = DayOfTheWeek.Tuesday;
            }
            else if (days.Wednesday == true)
            {
                dayofTheWeek = DayOfTheWeek.Wednesday;
            }
            else if (days.Thursday == true)
            {
                dayofTheWeek = DayOfTheWeek.Thursday;
            }
            else if (days.Friday == true)
            {
                dayofTheWeek = DayOfTheWeek.Friday;
            }
            else if (days.Saturday == true)
            {
                dayofTheWeek = DayOfTheWeek.Saturday;
            }
            else if (days.Sunday == true)
            {
                dayofTheWeek = DayOfTheWeek.Sunday;
            }
            else
            {
                dayofTheWeek = DayOfTheWeek.day;
            }
            return dayofTheWeek;
        }

        public static string XMLSerializer(object obj)
        {
            XmlSerializer xml = new XmlSerializer(obj.GetType());
            XmlDocument xmldoc = new XmlDocument();

            using (MemoryStream stream = new MemoryStream())
            {
                xml.Serialize(stream, obj);
                stream.Position = 0;
                xmldoc.Load(stream);
            }
            return xmldoc.InnerXml;
        }

        public static Schedule XMLDeserializer(string value)
        {
            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, IgnoreWhitespace = true, IgnoreComments = true };
            var xmlReader = XmlReader.Create(new StringReader(value), settings);
            xmlReader.Read();

            XmlSerializer deserializer = new XmlSerializer(typeof(Schedule));
            Schedule schedule = (Schedule)deserializer.Deserialize(xmlReader);
            return schedule;
        }

        public static string GetScheduleServiceName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["SchedulerServiceName"];
        }
    }
}