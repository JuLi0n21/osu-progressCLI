using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using System.Diagnostics;

namespace osu_progressCLI.server
{
    public sealed class PageGenerator
    {
        JObject user = null;
        Stopwatch cachetime;
        private static PageGenerator instance;

        private PageGenerator() { }

        public static PageGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PageGenerator();
                }
                return instance;
            }
        }

        public string generatepage(string userid, string mode, WeekCompare week)
        {
            try {

                var config = Credentials.Instance.GetConfig();

                Random rdm = new Random();

                string username = "Guest";
                string avatar_url = "https://osu.ppy.sh/images/layout/avatar-guest.png";
                string cover_url = "https://osu.ppy.sh/images/headers/profile-covers/c" + rdm.Next(1, 9).ToString() + ".jpg";
                string country = "Unknown";
                string countrycode = "USA";
                string rank = "-";
                string countryrank = "-";

                Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, $@" Screen: {week.Status} Lastweek: {week.LastWeek} ThisWeek: {week.ThisWeek}");
                string BanchoStatus = week.Status;
                string playtimethisweek = (week.ThisWeek / 3600).ToString().PadRight(5).Substring(0, 5);
                string diffrencetolastweek = ((week.ThisWeek - week.LastWeek) / week.LastWeek * 100).ToString().PadRight(6).Substring(0, 6);
                if (config.Localconfig == "False" || user == null) {
                  try
                    {
                        user = ApiController.Instance.getuser(userid, mode).Result;

                    } catch {
                        if (!string.IsNullOrEmpty(config.username))
                        {
                            username = config.username;
                        }

                        if (!string.IsNullOrEmpty(config.avatar_url))
                        {
                            avatar_url = config.avatar_url;
                        }

                        if (!string.IsNullOrEmpty(config.cover_url))
                        {
                            cover_url = config.cover_url;
                        }

                        if (!string.IsNullOrEmpty(config.country))
                        {
                            country = config.country;
                        }

                        if (!string.IsNullOrEmpty(config.rank))
                        {
                            rank = config.rank;
                        }
                    }

                    if (user != null)
                    {
                        username = user["username"]?.ToString();
                        avatar_url = user["avatar_url"]?.ToString();
                        cover_url = user["cover_url"]?.ToString();
                        country = user["country"]["name"]?.ToString();
                        countrycode = user["country"]["code"]?.ToString().ToLower();
                        rank = user["statistics"]["global_rank"]?.ToString();
                        countryrank = user["statistics"]["country_rank"]?.ToString();
                    }
                }
                else {

                    if (!string.IsNullOrEmpty(config.username))
                    {
                        username = config.username;
                    }

                    if (!string.IsNullOrEmpty(config.avatar_url))
                    {
                        avatar_url = config.avatar_url;
                    }

                    if (!string.IsNullOrEmpty(config.cover_url))
                    {
                        cover_url = config.cover_url;
                    }

                    if (!string.IsNullOrEmpty(config.country))
                    {
                        country = config.country;
                    }

                    if (!string.IsNullOrEmpty(config.rank))
                    {
                        rank = config.rank;
                    }
                }




                string html = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>osu!progress</title>
    <link href=""https://cdn.jsdelivr.net/npm/tailwindcss@2.2.15/dist/tailwind.min.css"" rel=""stylesheet"">
    <title>osu!progress</title>
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css"">
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/chartjs-plugin-trendline""></script>
    <script src=""https://cdn.jsdelivr.net/npm/flatpickr""></script>
    <script src=""https://cdn.jsdelivr.net/npm/moment""></script>
    <script src=""https://cdn.jsdelivr.net/npm/chartjs-adapter-moment@^1""></script>
    <link href=""https://cdnjs.cloudflare.com/ajax/libs/flowbite/1.8.1/flowbite.min.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/gh/lipis/flag-icons@6.11.0/css/flag-icons.min.css"" /> 
    <link rel=""stylesheet"" href=""style.css"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css"">
    <style>
       
    </style>
</head>
<body>


    <!-- Reload button -->
    <div class=""fixed top-10 right-20 z-40"">
        <button id=""loadDataButton"" class=""text-white rounded-full p-3 shadow-lg ml-4 mt-4 mb-4 backdrop--light"">
            <img src=""https://upload.wikimedia.org/wikipedia/commons/9/9a/Refresh_font_awesome.svg"" alt=""Refresh"" style=""width: 24px; height: 24px; filter: invert(100%);"" />
        </button>
    </div>

    <!-- Settings button -->
    <div class=""fixed top-10 right-0 z-40"">
        <button id=""settingsButton"" class=""text-white rounded-full p-3 z-30 shadow-lg mr-4 mt-4 backdrop--light"">
            <i class=""fa-solid fa-pen"" style=""color: #ffffff;""></i>
        </button>
    </div>

<!-- Settings panel (initially hidden) -->
<div id=""settingsPanel"" class=""fixed top-24 right-5 transform z-30 rounded-lg scale-0 transition-transform duration-300 ease-in-out text-white text backdrop--dark"">
    <i class=""fas fa-info-circle ml-2 text-blue-500 cursor-pointer hover:text-blue-700 top-0 right-0"" title=""Add informative text!""></i>

    <h2 class=""text-xl text-center font-semibold mb-4"">Settings</h2>

    <!-- Osu!folder -->
 <div class=""mb-3"">
        <label for=""textInput3"">Osu!folder:</label>
               <input type=""text"" id=""osufolder_input""  placeholder=""D://osu!"" value="""" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

    <!-- Song!folder -->
 <div class=""mb-3"">
        <label for=""textInput3"">Song!folder:</label>
         <input type=""text"" id=""songfolder_input""  placeholder=""C://osu!"" value="""" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

    <!-- Toggle 1 -->
    <div class=""flex items-center justify-between mb-3"">
        <span title=""Use Local config to set custom: Name, Rank, Banner, Avatar... (in case you're banned, for example)"">Local config:</span>
        <label class=""switch"">
            <input type=""checkbox"" id=""localsettingstoggle"" onchange=""toggleLocalConfigMenu()"">
            <span class=""slider round""></span>
        </label>
    </div>

    <!-- Menu to open -->
    <div class=""local-config-menu hidden"">
        <!-- Add your menu content here -->
        <p title=""Reload the Page after Saving"">Customize this Page.</p>
        
 <div class=""mb-3"">
        <label for=""textInput3"">Username:</label>
        <input type=""text"" id=""username_input""  placeholder=""cookieze"" value=""{username}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

 <div class=""mb-3"">
        <label for=""textInput3"">Rank:</label>
        <input type=""text"" id=""rank_input"" placeholder=""1"" value=""{rank}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

 <div class=""mb-3"">
        <label for=""textInput3"">Country:</label>
        <input type=""text"" id=""country_input"" placeholder=""Unknown"" value=""{country}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

 <div class=""mb-3"">
        <label for=""textInput3"">Avatarurl:</label>
        <input type=""text"" id=""avatarurl_input"" placeholder=""https://image.stern.de/8470686/t/zB/v2/w960/r1.7778/-/trump-als-clown.jpg"" value=""{avatar_url}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

 <div class=""mb-3"">
        <label for=""textInput3"">coverurl:</label>
        <input type=""text"" id=""coverurl_input"" placeholder=""https://i.gifer.com/origin/ec/ecd5961f42cedb3f4c6715fbddd39dc4_w200.webp"" value=""{cover_url}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

 <div class=""mb-3"">
        <label for=""textInput3"">Port</label>
        <input type=""text"" id=""port_input"" placeholder=""42069"" value=""{config.port}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>
    </div>

    <!-- Toggle 2 -->
    <div class=""flex items-center justify-between mb-3"">
        <span>Livestatusbar:</span>
        <label class=""switch"">
            <input type=""checkbox"" id=""livestatusbartoggle"">
            <span class=""slider round""></span>
        </label>
    </div>

    <!-- Text Input 3 -->
    <div class=""mb-3"">
        <label for=""userid"">Userid / Username:</label>
        <input type=""text"" id=""userid"" palceholder=""{userid}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

    <!-- Save button -->
   <div class=""flex justify-center"">
    <button id=""saveButton"" class=""bg-blue-500 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-full"" onclick=""saveSettings()"">
        Save
    </button>
    </div>
</div>

<!-- Recap -->
<!-- Live Status-->
    <div id=""status-bar"" class=""z-10 sticky top-0 w-full h-20 backdrop--medium text-white text-center p-2 hidden"">
        <span id=""status-text""></span>
<div id=""audio-bar"">
    <div id=""audio-time""></div>
    <div id=""audio-text""></div>

</div>

    </div>
    <!-- Main page-->
    <div class=""flex justify-center items-center"">

        <div class=""content w-1/2 rounded-lg backdrop--medium--dark text-white"">

            <!-- header -->
            <div id=""header"" class=""backdrop--medium justify-center items-center flex p-4 m-2"">
                <h1>Play History</h1>
            </div>

            <div class=""backdrop--light"">
                <div style=""background-size: cover; background-position: center center;background-image: url(&quot;{cover_url}&quot;); height: 250px"">

                </div>

            <div class=""flex"">
                <div class=""ml-60 text-left pt-6 pb-6"">
                    <p class=""usernameplaceholder"">{username}</p>  
                    <p class=""rankplaceholder"">#{rank} (#{countryrank})</p>
                    <p class=""countryplaceholder"">  <span class=""fi fi-{countrycode}""></span>{country}</p>
                </div>

                <div class=""ml-60 text-left pt-6 pb-6"">
                    <p class=""WastedTime"">⏰ {playtimethisweek}H ({diffrencetolastweek}%) [{BanchoStatus}]</p>  
                    <p class=""mostplayedscreen"">💻 {week.Screen}</p>
                </div>
            </div>

                <div style=""position: relative;"" class=""mb-7"">
                      <a href=""https://osu.ppy.sh/users/{userid}"" class="" hover:border-yellow-500  hover:border"" target=""_blank"" rel=""noopener noreferrer"">
                    <img src=""{avatar_url}"" style=""position: absolute; top: -150px; left: 60px; right: 0; height:120px; width:120px"" class=""rounded-lg"" />
                    </a>
                </div>

            </div>

<!-- Recent Scores -->
<div class=""flex rounded-t-lg justify-between backdrop--medium"">
      
        <div id=""filler"" class="""">
        </div>
            <h1 class=""m-4""></h1>
             <h1 class=""m-4"">Recent Scores</h1>
            <div>
                <input type=""text"" id=""mapsearch"" class=""text--yellow backdrop--medium border-b-4""  title=""Supported Terms cs, hp, od, ar, sr, acc, pp, fcpp, bpm!"">
            </div>

        </div>

    <div id=""scorecontainer"" class=""flex-col overflow-y-scroll backdrop--medium page-width max-h-96"">
    </div>
 <div class=""p-2 rounded-b-lg backdrop--medium""><h1></h1></div>

            <div id=""chart1"" class=""flex backdrop--medium mt-4 mb-3"" style=""height: 500px"">
                <div class=""w-1/2 rounded-l-lg backdrop--light flex-col justify-center items-center flex mt-4 m-2"">
                    <h2>BanchoTime</h2>
                    <canvas id=""BanchoTimeChart"" height=""440"" width=""450""></canvas>
                </div>
                <div class=""w-1/2 rounded-r-lg backdrop--light flex-col justify-center items-center flex mt-4 m-2"">
                    <h2>TimeWasted</h2>
                    <canvas id=""TimeWastedChart"" height=""440"" width=""450""></canvas>
                </div>
            </div>

            <div id=""chart2"" class=""border-b rounded-lg backdrop--light mb-4 p-4"">
                <div class=""flex-col justify-center items-center flex"">
                    <h1>Difficulties</h1>
                </div>
                    <div style=""height: 460px"">
                        <canvas id=""averageschart""></canvas>
                    </div>
                
            </div>

            <div id=""chart3"" class=""flex backdrop--medium"" style=""height: 280px"">
                <div class=""w-1/2 rounded-l-lg backdrop--light flex-col justify-center items-center flex m-4"">
                    <h2>BanchoTime</h2>
                    <canvas id=""pieBanchoTimeChart"" height=""250"" width=""450""></canvas>
                </div>
                <div class=""w-1/2 rounded-r-lg backdrop--light flex-col justify-center items-center flex m-4"">
                    <h2>TimeWasted</h2>
                    <canvas id=""pieTimeWastedChart"" height=""250"" width=""450""></canvas>
                </div>
            </div>

        </div>

    </div>

    <script src=""recentscores.js""></script>
    <script src=""configmenu.js""></script>
    <script src=""difficulities.js""></script>
    <script src=""savebutton.js""></script>
    <script src=""scoreexample.js""></script>
    <script src=""searchbar.js""></script>
    <script src=""timespend.js""></script>
    <script src=""timespendtotal.js""></script>
<script>     
    const socket = new WebSocket('ws://localhost:{Credentials.Instance.GetConfig().port}');
</script>
    <script src=""websocket.js""></script>
    <script>
   
document.getElementById('loadDataButton').addEventListener('click', function () {{
    {{
        loaddata();
    }}
}});



        //get data from internal api
        function loaddata() {{
            fetch('/api/beatmaps')
                .then(response => response.json())
                .then(data => {{
                   // console.log(data);
                  //  renderChart(data);
                    createScoreElements(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});

            fetch('/api/banchotime')
                .then(response => response.json())
                .then(data => {{
                    //console.log(data);
                    renderPie(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});

            fetch('/api/timewasted')
                .then(response => response.json())
                .then(data => {{
                    //console.log(data);
                    renderTimewastedPie(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});

            fetch('/api/beatmaps/averages')
                .then(response => response.json())
                .then(data => {{
                    //console.log(data);
                    createchart(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});
            fetch('/api/timewastedbyday')
                .then(response => response.json())
                .then(data => {{
                    //console.log(data);
                    createTimeWastedChart(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});
            fetch('/api/banchotimebyday')
                .then(response => response.json())
                .then(data => {{
                    //console.log(data);
                    createBanchoTimeChart(data);
                }})
                .catch(error => {{
                    console.error('Error loading data:', error);
                }});

 
        }}

    
        // Trigger the initial data load when the page loads (optional)
        window.addEventListener('DOMContentLoaded', function () {{
            // You can choose to load data immediately or wait for the button click
            // For example, you can uncomment the line below to load data on page load:
            // document.getElementById('loadDataButton').click();
            loaddata();
        }});
    

    </script>

</body>
</html>

            ";

                return html;
            } catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Network, e.Message);
                return "Please make sure ur internet is working";
            }
        }  
    } 
}
