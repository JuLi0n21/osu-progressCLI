function createScoreElements(scores) {
const scoresContainer =  document.getElementById("scorecontainer");
scoresContainer.innerHTML = "";
  scores.forEach((score) => {
    const scoreElement = document.createElement("div");
    scoreElement.className = "flex justify-center mb-0";
    
    score.Accuracy = score.Accuracy.toFixed(2);
    // Build the HTML structure for each score using the provided data
    scoreElement.innerHTML = 
        `<div class="flex backdrop--light h-16 rounded justify-between m-4 w-5/6 mb-1 mt-1">

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
                <p class="text-white">${score.Score} / ${score.MaxCombo} {${score.MaxCombo}}</p>
            </div>
            <div class="flex">
                <p class="text--dark--yellow">${score.Version}</p>
                <p class="text--gray ml-4">${score.Date}</p>
            </div>
        </div>

        <!-- ACC and Hits-->
        <div class="flex acc-container">
            <div class="flex flex-col justify-evenly justify-self-end rounded w-1/4 ml-3">
                <div>
                    <p class="text--yellow">${score.Accuracy}%</p>
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
</div>`;

    // Append the score element to the container
    scoresContainer.appendChild(scoreElement);
  });
}