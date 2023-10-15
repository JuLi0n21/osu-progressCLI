let beatmapid, rowid;
function fetchOsuBeatmap() {
    const urlParams = new URLSearchParams(window.location.search);
    const beatmapId = urlParams.get('id');
    
    if (!beatmapId) {
        document.getElementById('beatmapData').innerHTML = 'Beatmap ID not provided.';
        return;
    }

    rowid = beatmapId;
    const beatmapUrl = `/api/beatmaps/score?id=${beatmapId}`;

    fetch(beatmapUrl)
        .then(response => response.json())
        .then(data => {
            console.log(data)
            beatmapid = data.Beatmapid;

            document.getElementById('beatmapData').innerHTML = `
           <p class="relative top-4 left-4">${data.Status}</p>
            <div class="flex justify-center ml-4 mb-4 mr-4 text-xl">
              
        <h2>${data.Osufilename}</h2>

            </div>
         
<div>
  <div id="image-container" style="min-height: 280px; position: relative;">
    <img class="absolute top-0 left-0 w-40" src="${data.Grade}.png" alt="${data.Grade}">
    <a href="https://osu.ppy.sh/users/${data.Creator}" class="absolute bottom-10 right-0 score-backdrop--dark rounded-lg m-3 p-1 text--pink hover:text-white">${data.Version} by ${data.Creator}</a>
    <p class="absolute bottom-0 right-0 score-backdrop--dark rounded-lg m-3 p-1">${data.Username} at ${data.Date}</p>
    <div class="relative">
      <button id="playButton" class="absolute left-2 top-52 score-backdrop--dark text--pink hover:text-white">
        <i class="fas fa-play"></i>
      </button>
      <audio id="audioPlayer">
        <source src="https:${data.Preview}" type="audio/mpeg">
        Your browser does not support the audio element.
      </audio>

    </div>

    <div class="relative">
      <div class="abs-div" id="absDiv"></div>

      
    </div>
  
    <div>
    <a href="https://osu.ppy.sh/beatmapsets/${data.BeatmapSetid}#${data.Mode}/${data.Beatmapid}" target="_blank">
        <img src="${data.CoverList}">
        </a>
    </div>
  </div>
</div>

       
<div class="flex justify-evenly h-64 text-base">
  <div class="flex flex-col justify-evenly">
    <div class="m-8">


            <div class="flex justify-evenly">
          <div class="flex-col mb-4 flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              ★
            </div>
            <div class="flex justify-evenly">
              <p>${data.SR.toFixed(2)}</p>
            </div>
          </div>
             <div class="flex-col flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              AR
            </div>
            <div class="flex justify-evenly">
              <p>${data.Ar.toFixed(2)}</p>
            </div>
          </div>
             <div class="flex-col flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              CS
            </div>
            <div class="flex justify-evenly">
              <p>${data.Cs.toFixed(2)}</p>
            </div>
              
          </div>
                       <div class="flex-col flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              HP
            </div>
            <div class="flex justify-evenly">
              <p>${data.Hp.toFixed(2)}</p>
            </div>
              
          </div>
                       <div class="flex-col flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              OD
            </div>
            <div class="flex justify-evenly">
              <p>${data.Od.toFixed(2)}</p>
            </div>
              
          </div>
                       <div class="flex-col flex-1">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              BPM
            </div>
            <div class="flex justify-evenly">
              <p>${data.Bpm.toFixed(0)}</p>
            </div>
              
          </div>
        </div>



      <img src="${data.CoverList}" class="h-32 rounded-lg">
    </div>
  </div>
  <div class="flex flex-col justify-evenly w-2/5">
    <div class="w-92 m-8">
      <div class="flex justify-evenly"></div>
      <div class="flex-col">
        <div class="flex justify-evenly">
          <div class="flex-col mb-4">
            <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
              ACCURACY
            </div>
            <div class="flex justify-evenly">
              <p class="text--yellow">${data.Acc.toFixed(2)}%</p>
            </div>
          </div>
          <div class="flex-col flex-1">
            <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
              MAXCOMBO
            </div>
            <div class="flex justify-evenly">
                 <p>${data.Combo} {${data.MaxCombo}}</p>
            </div>
          </div>
            <div class="flex-col flex-1">
            <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
              Playtime
            </div>
            <div class="flex justify-evenly">
              ${data.Time}s  <p class="text--gray"> (${data.Playtype}) </p> 
            </div>
          </div>
        </div>
      </div>
      <!-- New Row (PP, Speed, Aim) -->
      <div class="flex justify-evenly">
        <div class="flex-col mb-4">
          <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
            PP
          </div>
          <div class="flex justify-evenly">
             <pre title="What the Play would have been Worth if it was ur best!" class="text--pink">${data.PP}</pre>
          </div>
        </div>
         <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            ACC
          </div>
          <div class="flex justify-evenly">
            ${data.ACCURACYATT}
          </div>
        </div>
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            SPEED
          </div>
          <div class="flex justify-evenly">
            ${data.SPEED}
          </div>
        </div>
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            AIM
          </div>
          <div class="flex justify-evenly">
            ${data.AIM}
          </div>
        </div>
        <div class="flex-col mb-4">
          <div class="flex justify-evenly flex-1 rounded-full score-backdrop--dark text-white pl-4 pr-4">
            FC
          </div>
          <div class="flex justify-evenly">
           <pre title="Full Combo (with given acc)" class="text--pink--dark">${data.FCPP}</pre>
          </div>
        </div>
      </div>
      <!-- New Rows (300, 100, 50, MISS) -->
      <div class="flex">
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            300
          </div>
          <div class="flex justify-evenly">
                 <pre class="text-blue-500">${data.Hit300}</pre> 
          </div>
        </div>
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            100
          </div>
          <div class="flex justify-evenly">
                 <pre class="text-green-500">${data.Hit100}</pre>
          </div>
        </div>
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white pl-4 pr-4">
            50
          </div>
          <div class="flex justify-evenly">
              <p class="text--orange">${data.Hit50}</p>
          </div>
        </div>
        <div class="flex-col flex-1">
          <div class="flex justify-evenly rounded-full score-backdrop--dark text-white">
            MISS
          </div>
          <div class="flex justify-evenly">
               <button id="MissAnalyzer">  <pre title="Open OsuMissAnalyzer!" class="text-red-600 hover:text-white">${data.HitMiss}↩</pre> </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>

<div class="flex">
            <a class="text--pink--dark" href="tags.html?tags=${data.Tags}" title="Tags">Tags</a>

                <a  class="text--pink" href="${data.CoverList}" target="_blank">
                    <pre> CoverList </pre>
                </a>

                <a class="text--pink" href="${data.Cover}" target="_blank">
                    <pre>Cover</pre>
                </a>
            </div>

        </div>
    </div>
`;      

            document.getElementById("MissAnalyzer").addEventListener("click", openAnalyzer)

            const playButton = document.getElementById("playButton");
            const audioPlayer = document.getElementById("audioPlayer");

            playButton.addEventListener("click", function () {
                event.preventDefault();
                if (audioPlayer.paused) {
                    audioPlayer.play();
                    playButton.innerHTML = '<i class="fas fa-pause"></i>';
                } else {
                    audioPlayer.pause();
                    audioPlayer.currentTime = 0;
                    playButton.innerHTML = '<i class="fas fa-play"></i>';
                }
            });

            const absDiv = document.getElementById("absDiv");
            absDiv.classList.add("absolute", "top-0", "right-0", "flex");
            const strings = data.ModsString.split(', ');
                const index = 20

                strings.forEach((str, index) => {
                    const img = document.createElement("img");
                    img.src = `${str}.png`;
                    img.alt = str;
                    absDiv.appendChild(img);

                    const width = (index + 1) * 95; 
                    absDiv.style.width = `${width}px`;
                });
            


            const apiUrl = `/api/beatmaps/search?searchquery=Beatmapid==${data.Beatmapid}`;

            fetch(apiUrl)
                .then(response => response.json())
                .then(data => {
                    console.log(data)
                    createScoreElements(data)
                    createLineChart('mapprogress', data, rowid);
                }).catch(error => {
                    console.error('Error fetching recent score data:', error);
                    document.getElementById('scorecontainer').innerHTML = 'An error occurred while fetching data.';
                });
        
        })
        .catch(error => {
            console.error('Error fetching beatmap data:', error);
            document.getElementById('beatmapData').innerHTML = 'An error occurred while fetching data.';
        });
}

function openAnalyzer() {
    console.log("MissAnalyzer Request");
    const apiUrl = "/api/run";

    const postData = {
        programm : "OsuMissAnalyzer",
        id : rowid
    };

    const requestOptions = {
        method: 'POST', 
        headers: {
            'Content-Type': 'application/json' 
        },
        body: JSON.stringify(postData) 
    };

    fetch(apiUrl, requestOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json(); 
        })
        .then(data => {
            console.log('POST request successful. Response data:', data);
        })
        .catch(error => {
            console.error('Error making POST request:', error);
        });
    alert("Starting OsuMissAnalyzer, please wait!");

}

function createLineChart(canvasId, data, highlightedScoreId) {
    const filteredData = data.filter(entry => entry.Time > 10);

    filteredData.reverse();

    console.log(highlightedScoreId)
    const dates = filteredData.map(entry => entry.Date);
    const accValues = filteredData.map(entry => entry.Acc);
    const ppValues = filteredData.map(entry => entry.PP);
    const comboValues = filteredData.map(entry => entry.Combo);

    const highlightedScoreIndex = highlightedScoreId !== undefined
        ? filteredData.findIndex(entry => entry.id == highlightedScoreId)
        : -1;

    console.log(filteredData)
    console.log(highlightedScoreId);
    console.log(highlightedScoreIndex);

    const ctx = document.getElementById(canvasId).getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: [
                {
                    label: 'Acc',
                    data: accValues,
                    borderColor: 'blue',
                    borderWidth: 2,
                    fill: false,
                    yAxisID: 'y', 
                },
                {
                    label: 'PP',
                    data: ppValues,
                    borderColor: 'green',
                    borderWidth: 2,
                    fill: false,
                    yAxisID: 'y2',
                },
                {
                    label: 'Combo',
                    data: comboValues,
                    borderColor: 'red',
                    borderWidth: 2,
                    fill: false,
                    yAxisID: 'y3',
                },
            ],
        },
        options: {
            responsive: true,
            plugins: {
                annotation: {
                    annotations: [{
                        type: 'line',
                        mode: 'vertical',
                        scaleID: 'x',
                        value: highlightedScoreIndex, 
                        borderColor: 'rgba(255, 255, 255, 0.1)',
                        borderWidth: 3,
                        label: {
                            content: 'Vertical Line',
                            enabled: true,
                            position: 'top'
                        }
                    }]
                }
            },
            legend: {
                labels: {
                    color: "rgba(255, 102, 171, 1)",
                    font: {
                        size: 14
                    }
                }
            },
            scales: {
                x: {
                    title: {
                        display: false,
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        display: true,
                    }
                },
                y: {
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Acc',
                        color: 'blue',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        display: false
                    }
                },
                y2: {
                    position: 'right',
                    title: {
                        display: true,
                        text: 'PP',
                        color: 'green',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        display: false
                    }
                },
                y3: {
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Combo',
                        color: 'red',
                    },
                    ticks: {
                        color: 'rgba(255, 102, 171, 1)'
                    },
                    grid: {
                        display: true,
                    }
                },
            },
        },
    });
}

function createScoreElements(scores) {
    const scoresContainer = document.getElementById("scorecontainer");
    scoresContainer.innerHTML = "";

    if (scores.length == 0) {
        scoresContainer.innerHTML = "<p>No Score Found!</p>";
    }
    scores.forEach((score) => {
        const scoreElement = document.createElement("a");
        scoreElement.className = "flex justify-center mb-0";

        score.Acc = score.Acc.toFixed(2);

        scoreElement.href = `/score.html?id=${score.id}`;
        scoreElement.target = "_blank";
        scoreElement.rel = "noopener noreferrer";

        scoreElement.innerHTML =
            `
        <div class="flex score-backdrop--light h-16 rounded justify-between m-4 w-5/6 mb-1 mt-1">
        <!-- Status and Grade-->
        <div class="flex flex-col grade-rank-container rounded justify-evenly w-1/6">
            <div class="flex justify-center">
                <p class="text--gray">${score.Status}</p>
            </div>
            <div class="flex justify-center">

                <img src="${score.Grade}.png" alt="${score.Grade}" class="w-20">
            </div>
        </div>
        <div class="backdrop--dark icon rounded-lg flex-nowrap">
            <img src="${score.Cover}" class="w-16 h-16" alt="?">
        </div>

        <!-- Name, Score/Combo, Grade Date -->
        <div class="flex flex-col rounded justify-evenly scoreinfo-container">
            <div>
                <p class="text-white whitespace-nowrap overflow-hidden">${score.Osufilename}</p>
            </div>
            <div>
                <p class="text-white whitespace-nowrap overflow-hidden">${score.Score} / ${score.Combo} {${score.MaxCombo}} ${score.ModsString}</p>
            </div>
            <div class="flex">
                <p class="text--dark--yellow whitespace-nowrap overflow-hidden">${score.Version}</p>
                <p class="text--gray ml-4">${score.Date} (${score.Playtype})</p>
            </div>
        </div>

        <!-- ACC and Hits-->
        <div class="flex acc-container">
            <div class="flex flex-col justify-evenly justify-self-end rounded w-1/4 ml-3">
                <div>
                    <p class="text--yellow">${score.Acc}%</p>
                </div>
                <div class="flex">
                    <p class="text-white">{</p>
                    <p class="text-blue-500">${score.Hit300}</p>
                    <p class="text-white">,</p>
                    <p class="text-green-500">${score.Hit100}</p>
                    <p class="text-white">,</p>
                    <p class="text--orange">${score.Hit50}</p>
                    <p class="text-white">,</p>
                    <p class="text-red-600">${score.HitMiss}</p>
                    <p class="text-white">}</p>
                </div>
            </div>
        </div>

        <!-- PP -->
        <div class="flex flex-col justify-evenly rounded backdrop--medium--light w-1/6 pp-container">
            <div class="flex justify-center">
                <p class="text--pink">${score.PP}pp</p>
            </div>
            <div class="flex justify-center">
                <p class="text--pink--dark justify-self-center">(${score.FCPP}pp)</p>
            </div>
        </div>
    </div>
</div>
</div>
`;

        scoresContainer.appendChild(scoreElement);
    });
}



const searchbar = document.getElementById('mapsearch');
let throttleTimeout;
let lastSearchText = '';

searchbar.addEventListener('input', function () {
    const searchText = searchbar.value.trim();

    if (searchText.length > 3 && searchText !== lastSearchText) {
        lastSearchText = searchText;

        scorecontainer.innerHTML = "";

        clearTimeout(throttleTimeout);
        throttleTimeout = setTimeout(() => {
            const apiUrl = 'api/beatmaps/search';

            fetch(`${apiUrl}?searchquery=${searchText} Beatmapid==${beatmapid}`)
                .then(response => response.json())
                .then(data => {
                    console.log(data);
                    createScoreElements(data);
                })
                .catch(error => {
                    console.error(error);
                });
        }, 500); 
    }

    if (searchText.length == 0) {

        const apiUrl = `/api/beatmaps/search?searchquery=Beatmapid==${beatmapid}`;

        fetch(apiUrl)
            .then(response => response.json())
            .then(data => {
                console.log(data);
                createScoreElements(data);
            })
            .catch(error => {
                console.error(error);
            });
    }
});

window.onload = fetchOsuBeatmap;