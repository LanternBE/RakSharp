using System.Net;
using RakSharp;

var address = new IPEndPoint(IPAddress.Any, 19132);
var server = new Server(address);

await server.Start();