# osu!progressCLI  
### THIS PROGRAMM IS IN EARLY ACCESS, BREAKING CHANGES ARE TO BE EXPECTED

CURRETN DESIGN (SUBJECT TO CHANGE)

![Imgur](https://i.imgur.com/u575NkG.png)

![Imgur](https://i.imgur.com/sA7HPQM.png)

![Imgur](https://i.imgur.com/XUIgJiq.png)

![Imgur](https://i.imgur.com/HgHABum.png)

This will change!
![Imgur](https://i.imgur.com/jmaEjyY.png)

## How to run
1. Download the zip
2. Extract it
3. run the osu!progressCLI.exe
4. Open ur Webbrowser and go to localhost:4200 (in cause its in use change it in the config.json add osu Songsfolder aswell (appears after first run)
5. CLick the Pen on the Website insert ur Clientid and ClientSecret and ur userid.
6. Hit save and refresh the Page ur Profile Stats should appear

## Requirments
- Only Windows supported currently (depends on OsuMemoryProvider)
- [NET 6.0.x Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) is required to run this programm

## Feedback / Problems
- Ask in the Discord
- Open An Issue 
- I got some changes/ features -> Make a pull request and maybe ill merge it
  
#EWWW the websites are fuckign ugly i dont like them
if u dont like the pages U CAN MAKE UR OWN ONE 

put the html/css/js/img files inside the public folders and u can fill them with ur own junk
### integrated api
to better enhance ur own webpages u can use the data from the internal api

current endpoints are 
- api/beatmaps/...
    - search
        - parameters searchquery="this is a search" 
    -  averages
    -  score
        - id  
- api/banchotime
- api/banchotimebyday
- api/timewasted
- api/timewastedbyday

## TODOs (no prioritys)

- [x] extract js and css out of the page generation (server side serving needed)
- [ ] Add possiblity to reload api data incase no internet (or beatmap changed)  (automatic or manual)
- [x] localbeatmap parser (offline, not submitted maps support) (use osu-tools) (bpm still missing)
- [ ] highconfigurable website (showing what graphs u want and what not)
- [ ] page for a single score
- [x] api endpoint for search functionaliy (add date functionaliy)
- [ ] make date picker work
- [x] mod support
- [ ] add local images in score displat website for mods and rankings (maybe rankedtype)
- [x] rename some Colums to enable "better" searching
- [x] example for costume webpages
- [ ] api documentaion / renaming
- [ ] importer for [circle-tracker](https://github.com/FunOrange/circle-tracker)
- [ ] add more stuff to the beatmap helper (images maybe) if someone asks or i need it i guess?

## NOTES
- rate limite api calls incause i ever make a importer (1 every 2 seconds) {OFFICAL OSU API! INTERNAL API DOES NOT HAVE LIMIT}
- change form .net 6.0 to somehting lower (to not have people install dependencys) (could nbot work cause osu tools is net6)
