﻿using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace osu_progressCLI.server
{
    public sealed class PageGenerator
    {
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

        public string generatepage(string userid, string mode)
        {
            Console.WriteLine(userid);
            JObject user = null;
            var config = Credentials.Instance.GetConfig();

            Random rdm  = new Random();

            string username = "Guest";
            string avatar_url = "https://osu.ppy.sh/images/layout/avatar-guest.png";
            string cover_url = "https://osu.ppy.sh/images/headers/profile-covers/c" + rdm.Next(1,9).ToString() + ".jpg";
            string country = "Unknown";
            string rank = "-";

            if (config.Localconfig == "False") {
                user = ApiController.Instance.getuser(userid, mode).Result;

                if (user != null)
                {
                    username = user["username"]?.ToString();
                    avatar_url = user["avatar_url"]?.ToString();
                    cover_url = user["cover_url"]?.ToString();
                    country = user["country"]["name"]?.ToString();
                    rank = user["statistics"]["global_rank"]?.ToString();
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
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css"">
    <script src=""https://cdn.jsdelivr.net/npm/chartjs-plugin-trendline""></script>
    <script src=""https://cdn.jsdelivr.net/npm/flatpickr""></script>
    <script src=""https://cdn.jsdelivr.net/npm/moment""></script>
    <script src=""https://cdn.jsdelivr.net/npm/chartjs-adapter-moment@^1""></script>
    <link href=""https://cdnjs.cloudflare.com/ajax/libs/flowbite/1.8.1/flowbite.min.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css"">
    <style>
        body {{
            margin: 0;
            padding: 0;
        }}

        .content {{
            max-width: 1000px;
            min-width: 1000px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
          
        }}


        @media (max-width: 949px) {{

            .inner-content {{
                margin: 10px 0;
            }}

            h2 {{
                font-size: 24px;
            }}

            p {{
                font-size: 14px;
                line-height: 1.3;
            }}
        }}

        /* Reset some default styles */
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: Arial, sans-serif;
            background-color: rgba(28,23,25,255);
        }}

        /* Header Styles */
        header {{
            background-color: #333;
            color: #fff;
            text-align: center;
            padding: 20px;
        }}


        .backdrop--light {{
            background-color: rgba(70,57,63,255);
        }}

        .backdrop--medium {{
            background-color: rgba(56,46,50,255)
        }}

        .backdrop--dark {{
            background-color: rgba(42,34,38,255);
        }}

          .backdrop--dark {{
            background-color: rgba(28, 23, 25, 1);
        }}

        .backdrop--medium--dark {{
            background-color: rgba(42, 34, 38, 1);
        }}

        .backdrop--medium {{
            background-color: rgba(56, 46, 50, 1);
        }}

        .backdrop--medium--light {{
            background-color: rgba(70, 57, 63, 1);
        }}

        .backdrop--light {{
            background-color: rgba(84, 69, 76, 1);
        }}

         .text--dark--yellow {{
            color: rgba(221, 159, 8, 255);
        }}

        .text--gray {{
            color: rgba(162, 142, 151, 255);
        }}

        .text--yellow {{
            color: rgba(252, 198, 51, 255);
        }}

        .text--pink {{
            color: rgba(255, 102, 171, 255);
        }}

        .text--orange {{
            color: #dc8726;
        }}

        .text--pink--dark {{
            color: #663e70;
        }}

        .page-width {{
            max-width: 1000px;
            min-width: 1000px;
        }}

        .grade-rank-container {{
            max-width: 100px;
            min-width: 100px;
        }}

        .icon {{
            max-width: 64px;
            min-width: 64px;

        }}

        .scoreinfo-container {{
            max-width: 400px;
            min-width: 400px;
        }}

        .acc-container {{
            max-width: 120px;
            min-width: 120px;
        }}

        .pp-container {{
            max-width: 100px;
            min-width: 100px;
        }}
        

    </style>
</head>
<body>


    <!-- Reload button -->
    <div class=""fixed top-0 right-20"">
        <button id=""loadDataButton"" class=""text-white rounded-full p-3 shadow-lg ml-4 mt-4 mb-4 backdrop--medium"">
            <img src=""https://upload.wikimedia.org/wikipedia/commons/9/9a/Refresh_font_awesome.svg"" alt=""Refresh"" style=""width: 24px; height: 24px; filter: invert(100%);"" />
        </button>
    </div>

    <!-- Settings button -->
    <div class=""fixed top-0 right-0"">
        <button id=""settingsButton"" class=""text-white rounded-full p-3 shadow-lg mr-4 mt-4 backdrop--medium"">
            <i class=""fa-solid fa-pen"" style=""color: #ffffff;""></i>
        </button>
    </div>

<!-- Settings panel (initially hidden) -->
<div id=""settingsPanel"" class=""fixed top-20 right-5 transform rounded-lg scale-0 transition-transform duration-300 ease-in-out text-white text backdrop--dark"">
    <i class=""fas fa-info-circle ml-2 text-blue-500 cursor-pointer hover:text-blue-700 top-0 right-0"" title=""Add informative text!""></i>

    <h2 class=""text-xl text-center font-semibold mb-4"">Settings</h2>

    <!-- ClientId -->
    <div class=""mb-3 flex items-center"">
        <label for=""ClientId"" class=""flex items-center m-0"" title=""This is your Client ID, which is used for authentication. Hover for more info."">
            ClientId
        </label>
        <a href=""https://osu.ppy.sh/home/account/edit#oauth"" target=""_blank"" rel=""noopener noreferrer"">
            <i class=""fas fa-external-link-alt ml-2 text-blue-500 cursor-pointer hover:text-blue-700"" title=""You can get your OAuth credentials here!""></i>
        </a>
    </div>
    <input type=""text"" id=""ClientId"" placeholder=""42069"" value=""{Credentials.Instance.GetClientId()}"" class=""w-full border rounded px-2 py-1 backdrop--light"">

    <!-- ClientSecret -->
    <div class=""mb-3 flex items-center"">
        <label for=""ClientSecret"" class=""flex items-center m-0"" title=""This is your Client Secret."">
            ClientSecret
            <i class=""fas fa-info-circle ml-2 text-blue-500 cursor-pointer hover:text-blue-700"" title=""Never Share your Credentials with anyone, these are stored locally!""></i>
        </label>
    </div>
    <input type=""password"" id=""ClientSecret"" placeholder=""*******************"" value=""{Credentials.Instance.GetClientSecret()}"" class=""w-full border rounded px-2 py-1 backdrop--light"">

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
        <span>Toggle 2:</span>
        <label class=""switch"">
            <input type=""checkbox"" id=""toggle2"">
            <span class=""slider round""></span>
        </label>
    </div>

    <!-- Text Input 3 -->
    <div class=""mb-3"">
        <label for=""userid"">Userid:</label>
        <input type=""text"" id=""userid"" palceholder=""{userid}"" class=""w-full border rounded px-2 py-1 backdrop--light"">
    </div>

    <!-- Save button -->
   <div class=""flex justify-center"">
    <button id=""saveButton"" class=""bg-blue-500 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-full"" onclick=""saveSettings()"">
        Save
    </button>
    </div>
</div>


    <div class=""flex justify-center items-center"">

        <div class=""content w-1/2 rounded-lg backdrop--medium--dark text-white"">

            <!-- header -->
            <div class=""backdrop--medium justify-center items-center flex p-4 m-2"">
                <h1>Play History</h1>
            </div>

            <div class=""backdrop--light"">
                <div style=""background-size: cover; background-position: center center;background-image: url(&quot;{cover_url}&quot;); height: 250px"">

                </div>

                <div class=""ml-60 text-left pt-6 pb-6"">
                    <p class=""usernameplaceholder"">{username}</p>  <p class=""rankplaceholder"">#{rank}</p>
                    <p class=""countryplaceholder"">{country}</p>
                </div>

                <div style=""position: relative;"" class=""mb-7"">
                      <a href=""https://osu.ppy.sh/users/{userid}"" target=""_blank"" rel=""noopener noreferrer"">
                    <img src=""{avatar_url}"" style=""position: absolute; top: -150px; left: 60px; right: 0; height:120px; width:120px"" class=""rounded-lg"" />
                    </a>
                </div>
            </div>

 <div class=""border-b rounded-lg backdrop--light mb-4 p-4"">

                <!-- filter menu -->
                <div class=""flex justify-evenly text-black
                    <input type=""text"" id=""from"">
                    <label for=""to"">Select Date and Time:</label>
                    <input type=""text"" id=""to"">
                  
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

    <div id=""scorecontainer"" class=""flex-col overflow-y-scroll backdrop--medium page-width max-h-96 rounded-b-lg"">
    </div>
   

            <!-- Charts -->
            <div class=""border-b rounded-lg backdrop--light mb-4 p-4"">
                <div style=""height: 460px"">
                    <canvas id=""myChart""></canvas>
                </div>
            </div>

            
            <div class=""flex backdrop--medium m-4"" style=""height: 460px"">
                <div class=""w-1/2 rounded-l-lg backdrop--light"">
                    <h2>BanchoTime</h2>
                    <canvas id=""BanchoTimeChart"" width=""450""></canvas>
                </div>
                <div class=""w-1/2 rounded-r-lg backdrop--light"">
                    <h2>TimeWasted</h2>
                    <canvas id=""TimeWastedChart"" width=""450""></canvas>
                </div>
            </div>

            <div class=""border-b rounded-lg backdrop--light mb-4 p-4"">
                <div style=""height: 460px"">
                    <canvas id=""averageschart""></canvas>
                </div>
            </div>

            <div class=""flex backdrop--medium m-4"" style=""height: 460px"">
                <div class=""w-1/2 rounded-l-lg backdrop--light"">
                    <h2>BanchoTime</h2>
                    <canvas id=""pieBanchoTimeChart"" width=""450""></canvas>
                </div>
                <div class=""w-1/2 rounded-r-lg backdrop--light"">
                    <h2>TimeWasted</h2>
                    <canvas id=""pieTimeWastedChart"" width=""450""></canvas>
                </div>
            </div>

        </div>

    </div>


    <script src=""https://cdnjs.cloudflare.com/ajax/libs/flowbite/1.8.1/flowbite.min.js""></script>
    <script src=""recentscores.js""></script>
    <script src=""configmenu.js""></script>
    <script src=""difficulities.js""></script>
    <script src=""savebutton.js""></script>
    <script src=""scoreexample.js""></script>
    <script src=""searchbar.js""></script>
    <script src=""timespend.js""></script>
    <script src=""timespendtotal.js""></script>
    <script>

        //datepicker
        flatpickr(""#from"", {{
            enableTime: true,
            dateFormat: ""Y-m-d H:i"",
        }});

        flatpickr(""#to"", {{
            enableTime: true,
            dateFormat: ""Y-m-d H:i"",
        }});

   
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
                    console.log(data);
                    renderChart(data);
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
                    console.log(data);
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
        }
    }
}
