const statusBar = document.getElementById('status-bar');
const statusText = document.getElementById('status-text');
const audioTimeElement = document.getElementById('audio-time');
const audioBarElement = document.getElementById('audio-bar');
const audioTextElement = document.getElementById('audio-text');
const toggle = document.getElementById('livestatusbartoggle');
let lastAudioTime = 0;
let lastMessageReceivedTime = Date.now();

toggle.addEventListener('change', function () {
    if (toggle.checked) {
        statusBar.classList.remove('hidden');
    } else {
        statusBar.classList.add('hidden');
    }
});

socket.addEventListener('open', (event) => {
    socket.send('Hello, server!');
    console.log('WebSocket connection established');
});

socket.addEventListener('message', (event) => {
    const data = event.data;
    //console.log('Received data:', data);

    try {
        const jsonData = JSON.parse(data);
        if (jsonData.Type === 'baseaddresses') {
            const songName = jsonData.Data.Beatmap.MapString;
            const ar = jsonData.Data.Beatmap.Ar;
            const cs = jsonData.Data.Beatmap.Cs;
            const hp = jsonData.Data.Beatmap.Hp;
            const od = jsonData.Data.Beatmap.Od;
            const audioTime = jsonData.Data.GeneralData.AudioTime;
            const totalAudioTime = jsonData.Data.GeneralData.TotalAudioTime;

            let platstats;
            let content;

            //rewirte 
            if (audioTime === lastAudioTime) {
                playstats = `Paused ${songName}`
            content = `<div class="flex justify-center"> 
                        AR: ${ar}, CS: ${cs}, HP: ${hp}, OD: ${od}
                    </div>
                    <div class="flex justify-evenly">
                        <div>
                            Screen: ${jsonData.Data.GeneralData.RawStatus}
                        </div>
                      
                       <div>
                       </div>
                       
                       <div>
                              Status: ${jsonData.Data.BanchoUser.BanchoStatus}
                        </div>
                    </div>`;
                

            } else if (jsonData.Data.GeneralData.RawStatus != 2) {
                playstats = `Listening to ${songName}`
                content = `<div class="flex justify-center"> 
                        AR: ${ar}, CS: ${cs}, HP: ${hp}, OD: ${od}
                    </div>
                    <div class="flex justify-evenly">
                        <div>
                            Screen: ${jsonData.Data.GeneralData.RawStatus}
                        </div>
                      
                       <div>
                       </div>
                       
                       <div>
                              Status: ${jsonData.Data.BanchoUser.BanchoStatus}
                        </div>
                    </div>`;
              
            } else if (jsonData.Data.GeneralData.RawStatus == 2) {
                playstats = `Playing ${songName}`;
                content =
                    `<div class="flex justify-center"> 
                    <div class="flex">
                        <div class="flex">
                            <pre class="text--yellow"> ${jsonData.Data.Player.Accuracy.toFixed(2)}%</pre>
                            <pre class="text-green-500"> ${jsonData.Data.Player.Hit100} </pre>
                            <pre class="text--orange"> ${jsonData.Data.Player.Hit50} </pre>
                            <pre class="text-red-500"> ${jsonData.Data.Player.HitMiss} </pre>
                            <pre class="text--ping"> ${jsonData.Data.Player.Score} </pre>
                       </div>
                    </div>
                </div>`
            }
            statusText.innerHTML = `${playstats} ${content}`;

            //INGAME


            lastAudioTime = audioTime;
            // Update the status text with the extracted information

            const audioBarWidth = (audioTime / totalAudioTime) * 100;
            audioTimeElement.style.width = audioBarWidth + '%';
            audioTextElement.innerHTML = `
            <div class="flex justify-between">
                <div class="justify-self-start">${Math.floor((audioTime / 1000) / 60)}:${Math.floor((audioTime / 1000) % 60).toString().padStart(2, '0').slice(0, 3) }  </div>
                <div class="justify-self-end">${Math.floor((totalAudioTime / 1000) / 60)}:${Math.floor((totalAudioTime / 1000) % 60).toString().padStart(2,'0').slice(0,3) }  </div>
            </div>`;

            lastMessageReceivedTime = Date.now();
            checkForInactivity();
        }
    } catch (error) {
        console.error('Error parsing JSON:', error);
    }
});



socket.addEventListener('error', (error) => {
    console.error('WebSocket error:', error);
});

socket.addEventListener('close', (event) => {
    if (event.wasClean) {
        console.log('WebSocket connection closed cleanly, code=' + event.code + ', reason=' + event.reason);
    } else {
        console.error('WebSocket connection abruptly closed');
    }
});

function showStatusBar(text) {
    statusText.textContent = text;
    statusBar.classList.remove('hidden');
}

function checkForInactivity() {
    const currentTime = Date.now();
    const elapsedTime = currentTime - lastMessageReceivedTime;

    if (elapsedTime >= 5000) {
        yourFunctionToCall();
    } else {
        setTimeout(checkForInactivity, 5000);
    }
}

checkForInactivity();

function yourFunctionToCall() {
    //Reset Status BAr
    audioTimeElement.style.width = 0 + '%';
    audioTextElement.innerHTML = ``;
    statusText.textContent = `Live Statusbar!`;
}


