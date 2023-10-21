const statusBar = document.getElementById('status-bar');
const statusText = document.getElementById('status-text');
const audioTimeElement = document.getElementById('audio-time');
const audioBarElement = document.getElementById('audio-bar');
const audioTextElement = document.getElementById('audio-text');
let lastAudioTime = 0;

socket.addEventListener('open', (event) => {
    socket.send('Hello, server!');
    console.log('WebSocket connection established');
});

socket.addEventListener('message', (event) => {
    const data = event.data;
    console.log('Received data:', data);

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
                playstats = "Paused"
                if (jsonData.Data.GeneralData.RawStatus == 2) {
                    content = `${songName}, AR: ${ar}, CS: ${cs}, HP: ${hp}, OD: ${od}`;
                    //colors and shit make divs (p)
                    content = `${songName} --- ACC:${jsonData.Data.Player.Accuracy.toFixed(2)} ${jsonData.Data.Player.Hit100} ${jsonData.Data.Player.Hit50} ${jsonData.Data.Player.HitMiss.toFixed(2)} | CS: ${cs}, HP: ${hp}, OD: ${od}`;
                } else {
                    content = `${songName}, AR: ${ar}, CS: ${cs}, HP: ${hp}, OD: ${od}`;
                }

            } else if (jsonData.Data.GeneralData.RawStatus != 2) {
                playstats = "Listening to"
                content = `${songName}`
            } else if (jsonData.Data.GeneralData.RawStatus == 2) {
                playstats = "Playing"
                content = `${songName}, AR: ${ar}, CS: ${cs}, HP: ${hp}, OD: ${od}`;
                //colors and shit make divs (p)
                content = `${songName} --- ACC:${jsonData.Data.Player.Accuracy.toFixed(2)} ${jsonData.Data.Player.Hit100} ${jsonData.Data.Player.Hit50} ${jsonData.Data.Player.HitMiss.toFixed(2)} | CS: ${cs}, HP: ${hp}, OD: ${od}`;
            }
            statusText.textContent = `${playstats} ${content}`;

            //ingame


            lastAudioTime = audioTime;
            // Update the status text with the extracted information

            const audioBarWidth = (audioTime / totalAudioTime) * 100;
            audioTimeElement.style.width = audioBarWidth + '%';
            audioTextElement.innerHTML = `
            <div class="flex justify-between">
                <div class="justify-self-start">${Math.floor((audioTime / 1000) / 60)}:${Math.floor((audioTime / 1000) % 60).toString().padStart(2, '0').slice(0, 3) }  </div>
                <div class="justify-self-end">${Math.floor((totalAudioTime / 1000) / 60)}:${Math.floor((totalAudioTime / 1000) % 60).toString().padStart(2,'0').slice(0,3) }  </div>
            </div>`;

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

function hideStatusBar() {
    statusBar.classList.add('hidden');
}

let isPlaying = true; // You can change this to control the song state
let songProgress = "300";
let songVolume = "100";
let songTempo = "50";
let songKey = "X";

function updateStatusBar() {
    if (isPlaying) {
        showStatusBar(`Playing (song ${songProgress} ${songVolume} ${songTempo} ${songKey})`);
    } else {
        showStatusBar("Paused");
    }
}

updateStatusBar();


