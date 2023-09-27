using Newtonsoft.Json.Linq;
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
    <script src=""https://cdn.jsdelivr.net/npm/flatpickr""></script>
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

        <div class=""content w-1/2 rounded-lg backdrop--dark text-white"">

            <!-- header -->
            <div class=""backdrop--medium p-4 m-2"">
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

<!-- Recent Scores -->
<div class""flex backdrop--medium"">
<h1  style=""text-align: center;"" >Recent Scores</h1>
</div>
    <div id=""scorecontainer"" class=""flex-col overflow-y-scroll page-width backdrop--medium max-h-96"">
</div>


    <div class=""border-b rounded-lg backdrop--light mb-4 p-4"">

                <!-- filter menu -->
                <div class=""flex justify-evenly text-black"">


                    <button id=""dropdownHelperButton"" data-dropdown-toggle=""dropdownHelper"" class=""text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center inline-flex items-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800"" type=""button"">
                        Dropdown checkbox <svg class=""w-2.5 h-2.5 ml-2.5"" aria-hidden=""true"" xmlns=""http://www.w3.org/2000/svg"" fill=""none"" viewBox=""0 0 10 6"">
                            <path stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""m1 1 4 4 4-4"" />
                        </svg>
                    </button>

                    <!-- Dropdown menu -->
                    <div id=""dropdownHelper"" class=""z-10 hidden bg-white divide-y divide-gray-100 rounded-lg shadow w-60 dark:bg-gray-700 dark:divide-gray-600"">
                        <ul class=""p-3 space-y-1 text-sm text-gray-700 dark:text-gray-200"" aria-labelledby=""dropdownHelperButton"">
                            <li>
                                <div class=""flex p-2 rounded hover:bg-gray-100 dark:hover:bg-gray-600"">
                                    <div class=""flex items-center h-5"">
                                        <input id=""helper-checkbox-1"" aria-describedby=""helper-checkbox-text-1"" type=""checkbox"" value="""" class=""w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-700 dark:focus:ring-offset-gray-700 focus:ring-2 dark:bg-gray-600 dark:border-gray-500"">
                                    </div>
                                    <div class=""ml-2 text-sm"">
                                        <label for=""helper-checkbox-1"" class=""font-medium text-gray-900 dark:text-gray-300"">
                                            <div>Enable notifications</div>
                                            <p id=""helper-checkbox-text-1"" class=""text-xs font-normal text-gray-500 dark:text-gray-300"">Some helpful instruction goes over here.</p>
                                        </label>
                                    </div>
                                </div>
                            </li>
                            <li>
                                <div class=""flex p-2 rounded hover:bg-gray-100 dark:hover:bg-gray-600"">
                                    <div class=""flex items-center h-5"">
                                        <input id=""helper-checkbox-2"" aria-describedby=""helper-checkbox-text-2"" type=""checkbox"" value="""" class=""w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-700 dark:focus:ring-offset-gray-700 focus:ring-2 dark:bg-gray-600 dark:border-gray-500"">
                                    </div>
                                    <div class=""ml-2 text-sm"">
                                        <label for=""helper-checkbox-2"" class=""font-medium text-gray-900 dark:text-gray-300"">
                                            <div>Enable 2FA auth</div>
                                            <p id=""helper-checkbox-text-2"" class=""text-xs font-normal text-gray-500 dark:text-gray-300"">Some helpful instruction goes over here.</p>
                                        </label>
                                    </div>
                                </div>
                            </li>
                            <li>
                                <div class=""flex p-2 rounded hover:bg-gray-100 dark:hover:bg-gray-600"">
                                    <div class=""flex items-center h-5"">
                                        <input id=""helper-checkbox-3"" aria-describedby=""helper-checkbox-text-3"" type=""checkbox"" value="""" class=""w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-700 dark:focus:ring-offset-gray-700 focus:ring-2 dark:bg-gray-600 dark:border-gray-500"">
                                    </div>
                                    <div class=""ml-2 text-sm"">
                                        <label for=""helper-checkbox-3"" class=""font-medium text-gray-900 dark:text-gray-300"">
                                            <div>Subscribe newsletter</div>
                                            <p id=""helper-checkbox-text-3"" class=""text-xs font-normal text-gray-500 dark:text-gray-300"">Some helpful instruction goes over here.</p>
                                        </label>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>



                    <input type=""text"" id=""from"">
                    <label for=""to"">Select Date and Time:</label>
                    <input type=""text"" id=""to"">
                    <input type=""text"" id=""mapsearch"">
                </div>
            </div>

            <!-- Charts -->
            <div class=""border-b rounded-lg backdrop--light mb-4 p-4"">
                <div style=""height: 460px"">
                    <canvas id=""myChart""></canvas>
                </div>
            </div>

            <div class=""flex backdrop--medium m-4"">
                <div class=""w-1/2 rounded-l-lg backdrop--light"">
                    <h2>BanchoTime</h2>
                    <canvas id=""pieChart"" width=""450""></canvas>
                </div>
                <div class=""w-1/2 rounded-r-lg backdrop--light"">
                    <h2>TimeWasted</h2>
                    <canvas id=""pieTimeWastedChart"" width=""450""></canvas>
                </div>
            </div>

        </div>

    </div>


    <script src=""https://cdnjs.cloudflare.com/ajax/libs/flowbite/1.8.1/flowbite.min.js""></script>
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

        //chart rendering
        function renderChart(data) {{
            const date = [];
            const BeatmapSetid = [];
            const Beatmapid = [];
            const Osufilename = [];
            const Ar = [];
            const Cs = [];
            const Hp = [];
            const Od = [];
            const Status = [];
            const StarRating = [];
            const Bpm = [];
            const Artist = [];
            const Creator = [];
            const Username = [];
            const Accuracy = [];
            const MaxCombo = [];
            const Score = [];
            const Combo = [];
            const Hit50 = [];
            const Hit100 = [];
            const Hit300 = [];
            const Ur = [];
            const HitMiss = [];
            const Mode = [];
            const Mods = [];

            // Extract the relevant data from the JSON
            data.forEach(entry => {{
                console.log(entry);
                date.push(entry.Date);
                BeatmapSetid.push(entry.BeatmapSetid);
                Beatmapid.push(entry.Beatmapid);
                Osufilename.push(entry.Osufilename.slice(0, 5) + ""..."");
                Ar.push(entry.Ar);
                Cs.push(entry.Cs);
                Hp.push(entry.Hp);
                Od.push(entry.Od);
                Status.push(entry.Status);
                StarRating.push(parseFloat(entry.StarRating));
                Bpm.push(entry.Bpm);
                Artist.push(entry.Artist);
                Creator.push(entry.Creator);
                Username.push(entry.Username);
                Accuracy.push(parseFloat(entry.Accuracy));
                MaxCombo.push(entry.MaxCombo);
                Score.push(entry.Score);
                Combo.push(entry.Combo);
                Hit50.push(entry.Hit50);
                Hit100.push(entry.Hit100);
                Hit300.push(entry.Hit300);
                Ur.push(entry.Ur);
                HitMiss.push(entry.HitMiss);
                Mode.push(entry.Mode);
                Mods.push(entry.Mods);
            }});


            const ctx = document.getElementById('myChart').getContext('2d');

            const validStarRatingValues = StarRating.filter(value => !isNaN(value));
            const starRatingAverage = (validStarRatingValues.reduce((sum, value) => sum + value, 0) / validStarRatingValues.length).toFixed(2);

            const validBpmValues = Bpm.filter(value => !isNaN(value));
            const BpmAverage = (validBpmValues.reduce((sum, value) => sum + value, 0) / validBpmValues.length).toFixed(2);

            console.log(starRatingAverage)
            const existingChart = Chart.getChart('myChart');
            if (existingChart) {{
                existingChart.destroy();
            }}

            const myChart = new Chart(ctx, {{
                type: 'bar',
                data: {{
                    labels: Osufilename,
                    datasets: [
                        {{
                            label: '50',
                            data: Hit50,
                            backgroundColor: 'rgba(220, 135, 39, 0.8)',
                            borderColor: 'rgba(0, 0, 0, 1)',
                            borderWidth: 1,
                            yAxisID: 'y',
                            order: 1,
                        }},
                        {{
                            label: '100',
                            data: Hit100,
                            backgroundColor: 'rgba(63, 220, 39, 0.8)',
                            borderColor: 'rgba(0, 0, 0, 1)',
                            borderWidth: 1,
                            yAxisID: 'y',
                            order: 1,
                        }},
                        {{
                            label: 'X',
                            data: HitMiss,
                            backgroundColor: 'rgba(204, 19, 19, 0.8)',
                            borderColor: 'rgba(0, 0, 0, 1)',
                            borderWidth: 1,
                            yAxisID: 'y',
                            order: 1,
                        }},
                        {{
                            label: 'Stars',
                            data: StarRating,
                            backgroundColor: 'rgba(204, 19, 19, 0.8)',
                            borderColor: 'rgba(0, 0, 0, 1)',
                            borderWidth: 1,
                            type: 'scatter',
                            yAxisID: 'y2',
                            order: 0,
                        }},
                        {{
                            label: 'Bpm',
                            data: Bpm,
                            backgroundColor: 'rgba(204, 19, 19, 0.8)',
                            borderColor: 'rgba(0, 0, 0, 1)',
                            borderWidth: 1,
                            type: 'scatter',
                            yAxisID: 'y3',
                            order: 0,
                        }},
                        {{
                            data: Array(StarRating.length).fill(starRatingAverage),
                            fill: false,
                            borderColor: 'rgba(250, 192, 32, 0.8)',
                            borderWidth: 3,
                            type: 'line',
                            yAxisID: 'y4',
                            order: 0,
                            pointRadius: 0,
                            label: ""Stars average"",
                        }},
                        {{
                            data: Array(Bpm.length).fill(BpmAverage),
                            fill: false,
                            borderColor: 'rgba(250, 192, 32, 0.8)',
                            borderWidth: 3,
                            type: 'line',
                            yAxisID: 'y5',
                            order: 0,
                            pointRadius: 0,
                            label: ""Bpm average"",
                        }},

                    ]
                }},
                options: {{
                    plugins: {{
                        title: {{
                            display: true,
                            text: 'ScoreExample'
                        }},
                        legend: {{

                        }},
                    }},
                    responsive: true,
                    scales: {{
                        x: {{
                            stacked: true,
                        }},
                        y: {{
                            stacked: true
                        }},
                        y2: {{
                            position: 'right',

                            ticks: {{
                                color: 'rgba(204, 19, 19, 0.8)'
                            }},
                            grid: {{
                                drawOnChartArea: false // only want the grid lines for one axis to show up
                            }}
                        }},
                        y3: {{
                            position: 'right',

                            ticks: {{
                                color: 'rgba(243, 19, 19, 0.8)'
                            }},
                            grid: {{
                                drawOnChartArea: false // only want the grid lines for one axis to show up
                            }}
                        }},
                        y4: {{

                            position: 'right',
                            suggestedMax: Math.max(StarRating), // Adjust to your data's max value
                            suggestedMin: 0,
                            ticks: {{
                                color: 'rgba(250, 192, 32, 0.8)',
                                display: false, // Hide ticks for this axis
                            }},
                            grid: {{
                                drawOnChartArea: false,
                            }},
                        }},
                        y5: {{

                            position: 'right',
                            suggestedMax: Math.max(Bpm), // Adjust to your data's max value
                            suggestedMin: 0,
                            ticks: {{
                                color: 'rgba(250, 192, 32, 0.8)',
                                display: false, // Hide ticks for this axis
                            }},
                            grid: {{
                                drawOnChartArea: false,
                            }},
                        }},
                    }}
                }}
            }});
        }}

        function renderPie(data) {{
            // Extract labels and values from the fetched data
            const labels = [];
            const values = [];

            for (let i = 0; i < data.length; i++) {{
                const item = data[i];
                if (item.Key !== """" && item.Value >= 10) {{
                    labels.push(item.Key);
                    item.Value = item.Value / 3600;
                    values.push(item.Value);
                }}
            }}

            // Create an array of random colors for each segment
            const colors = values.map(() => '#' + (Math.random() * 0xFFFFFF << 0).toString(16));

            console.log(labels, values)
            // Data for the pie chart
            const pieData = {{
                labels: labels,
                datasets: [{{
                    data: values,
                    backgroundColor: colors,
                }}]
            }};

            // Options for the pie chart
            const options = {{
                responsive: false,
                maintainAspectRatio: false,
                plugins: {{
                    legend: {{
                        display: true, // Set to true to display the legend
                        position: 'right', // Position of the legend (you can adjust it as needed)
                    }},
                }},

            }};

            // Get the canvas element
            const pie = document.getElementById('pieChart').getContext('2d');


            const existingChart = Chart.getChart('pieChart');
            if (existingChart) {{
                existingChart.destroy();
            }}

            const myChart = new Chart(pie, {{
                type: 'pie',
                data: pieData,
                options: options
            }});
        }}

        function renderTimewastedPie(data) {{
            // Extract labels and values from the fetched data
            const labels = [];
            const values = [];
            const colors = [];

            for (let i = 0; i < data.length; i++) {{
                const item = data[i];
                if (item.Key !== """" && item.Value >= 10) {{
                    labels.push(item.Key);
                    item.Value = item.Value / 3600;
                    values.push(item.Value);
                    colors.push('#' + (Math.random() * 0xFFFFFF << 0).toString(16));
                }}
            }}

            // Create an array of random colors for each segment

            console.log(labels, values)
            // Data for the pie chart
            const pieData = {{
                labels: labels,
                datasets: [{{
                    data: values,
                    backgroundColor: colors,
                }}]
            }};

            // Options for the pie chart
            const options = {{
                responsive: false,
                maintainAspectRatio: false,
                plugins: {{
                    legend: {{
                        display: true, // Set to true to display the legend
                        position: 'right', // Position of the legend (you can adjust it as needed)
                    }},
                }},
            }};

            // Get the canvas element
            const pie = document.getElementById('pieTimeWastedChart').getContext('2d');

            const existingChart = Chart.getChart('pieTimeWastedChart');
            if (existingChart) {{
                existingChart.destroy();
            }}

            const myChart = new Chart(pie, {{
                type: 'pie',
                data: pieData,
                options: options
            }});
        }}

        document.getElementById('loadDataButton').addEventListener('click', function () {{
            loaddata();
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
                    console.log(data);
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
        }}

        //settings menu
        const settingsButton = document.getElementById(""settingsButton"");
        const settingsPanel = document.getElementById(""settingsPanel"");
        const saveButton = document.getElementById(""saveButton"");
        const toggle1 = document.getElementById(""toggle1"");
        const toggle2 = document.getElementById(""toggle2"");
        const textInput1 = document.getElementById(""textInput1"");
        const textInput2 = document.getElementById(""textInput2"");
        const textInput3 = document.getElementById(""textInput3"");

        settingsButton.addEventListener(""click"", () => {{
            if (settingsPanel.classList.contains(""scale-0"")) {{
                settingsPanel.classList.remove(""scale-0"");
                settingsPanel.classList.add(""scale-100"");
            }} else {{
                settingsPanel.classList.remove(""scale-100"");
                settingsPanel.classList.add(""scale-0"");
            }}
        }});

         function toggleLocalConfigMenu() {{
                const menu = document.querySelector('.local-config-menu');
                const toggleCheckbox = document.getElementById('localsettingstoggle');
        
                if (toggleCheckbox.checked) {{
                    menu.classList.remove('hidden');
                }} else {{
                    menu.classList.add('hidden');
                }}
            }}

            //save configsettings
        
            function saveSettings() {{
        const clientId = document.getElementById('ClientId').value;
        const clientSecret = document.getElementById('ClientSecret').value;
        const username = document.getElementById('username_input').value;
        const rank = document.getElementById('rank_input').value;
        const country = document.getElementById('country_input').value;
        const avatarUrl = document.getElementById('avatarurl_input').value;
        const coverUrl = document.getElementById('coverurl_input').value;
        const port = document.getElementById('port_input').value;
        const userid = document.getElementById('userid').value; 
        const localConfigEnabled = document.getElementById('localsettingstoggle').checked;
        
        const data = {{
            clientId: clientId,
            clientSecret: clientSecret,
            username: username,
            rank: rank,
            country: country,
            avatarUrl: avatarUrl,
            coverUrl: coverUrl,
            port: port,
            localsettings: localConfigEnabled,
            userid: userid
        }};
               
        //console.log(data);

        // Send a POST request to the api/save endpoint
        fetch('api/save', {{
            method: 'POST',
            headers: {{
                'Content-Type': 'application/json'
            }},
            body: JSON.stringify(data)
        }})
        .then(response => {{
            if (response.ok) {{
                alarm('Settings saved successfully');
            }} else {{
                console.error('Failed to save settings');
            }}
        }})
        .catch(error => {{
            alarm('An error occurred:', error);
        }});
    }}


const searchbar = document.getElementById('mapsearch');
let throttleTimeout;
let lastSearchText = '';

searchbar.addEventListener('input', function() {{
  const searchText = searchbar.value.trim();

  if (searchText.length > 5 && searchText !== lastSearchText) {{
    lastSearchText = searchText;

    clearTimeout(throttleTimeout);
    throttleTimeout = setTimeout(() => {{
      const apiUrl = 'api/beatmaps/search';

      fetch(`${{apiUrl}}?searchquery=${{searchText}}`)
        .then(response => response.json())
        .then(data => {{
          console.log(data);
          createScoreElements(data);
        }})
        .catch(error => {{
          console.error(error);
        }});
    }}, 2000); // Delay for 2 seconds (2000 milliseconds)
  }}
}});


function createScoreElements(scores) {{
const scoresContainer =  document.getElementById(""scorecontainer"");
scoresContainer.innerHTML = """";
  scores.forEach((score) => {{
    const scoreElement = document.createElement(""div"");
    scoreElement.className = ""flex justify-center mb-0"";
    
    score.Accuracy = score.Accuracy.toFixed(2);
    // Build the HTML structure for each score using the provided data
    scoreElement.innerHTML = 
        `<div class=""flex backdrop--light h-16 rounded justify-between m-4 w-5/6 mb-1 mt-1"">

        <!-- Status and Grade-->
        <div class=""flex flex-col grade-rank-container rounded justify-evenly w-1/6"">
            <div class=""flex justify-center"">
                <p class=""text--gray"">${{score.Status}}</p>
            </div>
            <div class=""flex justify-center"">

                <img src=""https://osu.ppy.sh/assets/images/GradeSmall-A.d785e824.svg"" alt=""${{score.Grade}}"" class=""w-20"">
            </div>
        </div>
        <div class=""backdrop--dark icon rounded-lg flex-nowrap"">
            <img src=""${{score.Cover}}"" class=""w-16 h-16"" alt=""?"">
        </div>

        <!-- Name, Score/Combo, Grade Date -->
        <div class=""flex flex-col rounded justify-evenly scoreinfo-container"">
            <div>
                <p class=""text-white whitespace-nowrap overflow-hidden"">${{score.Osufilename}}</p>
            </div>
            <div>
                <p class=""text-white"">${{score.Score}} / ${{score.MaxCombo}} {{${{score.MaxCombo}}}}</p>
            </div>
            <div class=""flex"">
                <p class=""text--dark--yellow"">${{score.Version}}</p>
                <p class=""text--gray ml-4"">${{score.Date}}</p>
            </div>
        </div>

        <!-- ACC and Hits-->
        <div class=""flex acc-container"">
            <div class=""flex flex-col justify-evenly justify-self-end rounded w-1/4 ml-3"">
                <div>
                    <p class=""text--yellow"">${{score.Accuracy}}%</p>
                </div>
                <div class=""flex"">
                    <p class=""text-white"">{{</p>
                    <p class=""text-blue-500"">${{score.Hit300}}</p>
                    <p class=""text-white"">,</p>
                    <p class=""text-green-500"">${{score.Hit100}}</p>
                    <p class=""text-white"">,</p>
                    <p class=""text--orange"">${{score.Hit50}}</p>
                    <p class=""text-white"">,</p>
                    <p class=""text-red-600"">${{score.HitMiss}}</p>
                    <p class=""text-white"">}}</p>
                </div>
            </div>
        </div>

        <!-- PP -->
        <div class=""flex flex-col justify-evenly rounded backdrop--medium--light w-1/6 pp-container"">
            <div class=""flex justify-center"">
                <p class=""text--pink"">${{score.PP}}pp</p>
            </div>
            <div class=""flex justify-center"">
                <p class=""text--pink--dark justify-self-center"">(${{score.FCPP}}pp)</p>
            </div>
        </div>
    </div>
</div>
</div>`;

    // Append the score element to the container
    scoresContainer.appendChild(scoreElement);
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
