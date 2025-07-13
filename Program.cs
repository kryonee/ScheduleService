using Schedule.Controllers;

public class Program
{
    public static void Main()
    {
        var scheduleController = new ScheduleController();
        scheduleController.GenerateSchedule();
    }
}
