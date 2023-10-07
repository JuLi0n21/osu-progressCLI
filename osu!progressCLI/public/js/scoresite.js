let beatmapid;
function fetchOsuBeatmap() {
    const urlParams = new URLSearchParams(window.location.search);
    const beatmapId = urlParams.get('id');

    if (!beatmapId) {
        document.getElementById('beatmapData').innerHTML = 'Beatmap ID not provided.';
        return;
    }

    const beatmapUrl = `/api/beatmaps/score?id=${beatmapId}`;

    fetch(beatmapUrl)
        .then(response => response.json())
        .then(data => {
            console.log(data)
            beatmapid = data.Beatmapid;

            document.getElementById('beatmapData').innerHTML = `
            <div class="flex justify-center m-4 text-xl">
        <h2>${data.Osufilename}</h2>

            </div>
         
    <a href="https://osu.ppy.sh/beatmapsets/${data.BeatmapSetid}#osu/${data.Beatmapid}" target="_blank">
      <div id="imageplaceholder" style="min-height: 280px">
          <img class="absolute top:-40 left:0 w-40" src="${data.Grade}.png" alt="${data.Grade}">
 

        <div class="relative">

                <button id="playButton" class=" absolute  left-2 top-52 text-white backdrop--medium">
                    <i class="fas fa-play"></i>
                </button>
                    <audio id="audioPlayer">
                        <source src="https:${data.Preview}" type="audio/mpeg">
                        Your browser does not support the audio element.
                    </audio>
        </div>

        <div class="relative">
        <div class="abs-div" id="absDiv">


     </div>

 

     </div>

      <img src="${data.CoverList}">

      </div>
    </a>
       
    <div class="flex justify-between h-80"> 

        <div class="flex flex-col justify-between "> 

            <div class="flex flex-col">
                <p>${data.Status}</p>
                <p>AR: ${data.Ar.toFixed(2) }</p>
                <p>CS: ${data.Cs.toFixed(2) }</p>
                <p>HP: ${data.Hp.toFixed(2) }</p>
                <p>OD: ${data.Od.toFixed(2) }</p>
                <p>BPM: ${data.Bpm}</p>
                <p>SR: ${data.SR}</p>
            </div>

            <div></div>

            <div class="flex flex-col">
                <p>Artist: ${data.Artist}</p>
                <p>Creator: ${data.Creator}</p>
                <p>Version: ${data.Version}</p>
                <a class="w-1/2 text--pink--dark" href="tags.html?tags=${data.Tags}" title="Tags">Tags</a>

            </div>
        
            <div class="flex">
                <a  class="text--pink" href="${data.CoverList}" target="_blank">
                    <pre>CoverList </pre>
                </a>

                <a class="text--pink" href="${data.Cover}" target="_blank">
                    <pre>Cover</pre>
                </a>
            </div>
        
    </div>

        <div class="flex flex-col justify-between w-1/3"> 

        <div class="flex flex-col">
            <p>${data.Username}</p> 
            <p>Playtime: ${data.Time}s</p>
            <p>Score: ${data.Score}</p>
            <p>Combo: ${data.Combo}/${data.MaxCombo}</p>
            
            <div class="flex">
                <pre class="">Accuracy: </pre>
                <p class="text--yellow">${data.Acc.toFixed(2)}%</p>
            </div>
            
            <div class="flex">
                <pre class="">Hits: ${data.Hit50}</pre>
                 <pre class="text-green-500"> ${data.Hit100}</pre>
                 <pre class="text-blue-500"> ${data.Hit300}</pre> 
                 <pre class="text-red-600"> ${data.HitMiss}</pre>
            </div>
               <p title="NOT IMPLEMENTED">UR ${data.Ur}</p>
            </div>
           

            <div class="flex flex-col">
                <div class="flex">
                     <pre title="Achieved PP"PP: class="text--pink">${data.PP} </pre>
                     <p>/</p>
                     <pre title="Full Combo (with given acc)" class="text--pink--dark"> ${data.FCPP}</pre>
                </div>

                <div class="flex">
                    <pre title="Aim PP">${data.AIM} </pre>
                    <pre title="Speed PP">${data.SPEED} </pre>
                    <pre title="Accuracy PP">${data.ACCURACYATT}</pre>
                </div>
               
                </div>
                <div>
                     <p class="text-gray-600">${data.Date}</p>
                </div>
        </div>
    </div>
`;      

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
        <div class="flex backdrop--light h-16 rounded justify-between m-4 w-5/6 mb-1 mt-1">
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
                <p class="text-white whitespace-nowrap overflow-hidden">${score.Score} / ${score.MaxCombo} {${score.MaxCombo}} ${score.ModsString}</p>
            </div>
            <div class="flex">
                <p class="text--dark--yellow whitespace-nowrap overflow-hidden">${score.Version}</p>
                <p class="text--gray ml-4">${score.Date}</p>
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