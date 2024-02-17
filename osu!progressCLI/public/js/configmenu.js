// Settings menu
const settingsButton = document.getElementById("settingsButton");
const settingsPanel = document.getElementById("settingsPanel");
const saveButton = document.getElementById("saveButton");
const toggle1 = document.getElementById("toggle1");
const toggle2 = document.getElementById("toggle2");
const textInput1 = document.getElementById("textInput1");
const textInput2 = document.getElementById("textInput2");
const textInput3 = document.getElementById("textInput3");

settingsButton.addEventListener("click", () => {
  if (settingsPanel.classList.contains("scale-0")) {
    settingsPanel.classList.remove("scale-0");
    settingsPanel.classList.add("scale-100");
  } else {
    settingsPanel.classList.remove("scale-100");
    settingsPanel.classList.add("scale-0");
  }
});

function toggleLocalConfigMenu() {
  const menu = document.querySelector(".local-config-menu");
  const toggleCheckbox = document.getElementById("localsettingstoggle");

  if (toggleCheckbox.checked) {
    menu.classList.remove("hidden");
  } else {
    menu.classList.add("hidden");
  }
}
