using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var conn = new HubConnection("http://localhost:61650/");
            conn.Closed += async () =>
            {
                await Task.Delay(5000);
                try
                {
                    await conn.Start();
                }
                catch
                {
                }
            };

            var userHub = new UserHubProxy(conn.CreateHubProxy("UserHub"));
            var meetingHub = new MeetingHubProxy(conn.CreateHubProxy("MeetingHub"));


            meetingHub.OnCreate(meeting =>
            {
                Console.WriteLine($"Create: {meeting}");
            });
            meetingHub.OnUpdate(meeting =>
            {
                Console.WriteLine($"Update: {meeting}");
            });

            await conn.Start();

            var result = await userHub.CreateOrUpdate(new User { FacebookProfileId = "Test2", Name = "BBB" });
            //var result2 = await userHub.CreateOrUpdate(new User { GuestId = Guid.NewGuid(), Name = "ccc" });

            var users = await userHub.GetUsers();
            foreach (var u in users)
            {
                Console.WriteLine($"{u.FacebookProfileId}, {u.Name}");
            }

            foreach (var m in await meetingHub.GetMeetings())
            {
                Console.WriteLine(m);
            }

            //var result2 = await meetingHub.Create(new Meeting
            //{
            //    Name = $"Meeting {DateTime.Now}",
            //    Tickets = 1,
            //    Seconds = 30,
            //    Coupons = 0,
            //    CreatedBy = users.First().FacebookProfileId,
            //});

            Console.ReadKey();
            conn.Stop();
        }
    }


}
