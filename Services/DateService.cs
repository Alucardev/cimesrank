namespace ApiRanking.Services
{
    public class DateService
    {
        public static string GetPreviousMonthDate(string currentDate)
        {
            if (!DateTime.TryParse(currentDate, out var currentDateTime))
            {
                throw new ArgumentException("Formato de fecha inválido", nameof(currentDate));
            }

            var previousMonth = currentDateTime.AddMonths(-1);
            int day = Math.Min(currentDateTime.Day, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month));

            return new DateTime(previousMonth.Year, previousMonth.Month, day).ToString("dd/MM/yyyy");
        }

        public static string GetYesterdayDate()
        {
            var yesterday = DateTime.Now.AddDays(-1);
            return yesterday.ToString("dd/MM/yyyy");
        }
    }
}
