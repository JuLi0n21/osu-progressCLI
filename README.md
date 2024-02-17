# osu!progressCLI  [![CodeFactor](https://www.codefactor.io/repository/github/juli0n21/osu-progresscli/badge)](https://www.codefactor.io/repository/github/juli0n21/osu-progresscli) [![GitHub release](https://img.shields.io/github/release/juli0n21/osu-progresscli.svg)](https://GitHub.com/juli0n21/osu-progresscli/releases/) [![Github all releases](https://img.shields.io/github/downloads/JuLi0n21/osu-progresscli/total.svg)](https://GitHub.com/JuLi0n21/osu-progresscli/releases/) [![Discord](https://badgen.net/discord/members/9rUMkHJEvv)](https://discord.gg/9rUMkHJEvv)

Prerealse versions:  [![Github all actions](https://github.com/Juli0n21/osu-progresscli/actions/workflows/build-dev.yml/badge.svg)](https://GitHub.com/JuLi0n21/osu-progresscli/actions/)
A small Tool to Visualize localy Tracked scores for the osu! game.

<details>
  <summary>Homepage</summary>

![Imgur](https://i.imgur.com/TOq4cZL.png)

</details>

<details>
  <summary>Scorepage</summary>

![Imgur](https://i.imgur.com/e8jnW7b.png)

</details>

<details>
  <summary>Importer</summary>

![Imgur](https://i.imgur.com/ETXmREw.png)

</details>

### Feature List
- Automaticaly Tracks Passed, Failed, Retrys and Canceld Scores
- Calculates PP and Fullcombo PP
- Tracks Time spend on Any specific Screen
- Tracks BanchoTime (Idle, playing, afk...)
- More or less supports All Modes and Mods
- Shows Potential Fcable Maps
- Progression over time.
- Missanalzer for Passed Scores

## How to run

1. Download the [Latest Release](https://github.com/juli0n21/osu-progresscli/releases/)
2. Extract it
3. Run the Programm and open the [Webpage](http://localhost:4200)
4. Add youre Userid, Osu folder and song folder under the Pen icon hit save and refresh
5. If ur profile stats are displayed ur done!

6. If u wish to keep scores from older version, move the osu!progress.db to the new folder
7. If u want to Import Scores from ur current game or from osu!alternative visit  [importer](http://localhost:4200/importer)

## Requirments
- Only Windows supported currently (depends on OsuMemoryProvider, Missanalzer, etc...)
- [NET 6.0.x Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) is required to run this programm

## Feedback / Problems
- Ask in the [Discord](https://discord.gg/FtF2HNSJNC)
- Open An Issue 
- I got some changes/ features -> Make a pull request and maybe ill merge it

## I dont like this design
If u dont like the design u can change ur the existign one or create an etirely new one
<details>
  <summary>How to</summary>

If u need help hit me up!

### File based Routing
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


## NOTES
- Please take official api request limits into account!
one request every 2 seconds is ok, burst should be ok but if it goes to high ask ppy!

</details>

###
<details>
  <summary>TODOs. Is never Uptodate!</summary>
- [ ] Add possiblity to reload api data incase no internet (or beatmap changed)  (automatic or manual)
- [x] localbeatmap parser (offline, not submitted maps support) (use osu-tools) (bpm still missing)
- [ ] highconfigurable website (showing what graphs u want and what not)
- [ ] page for a single score
- [ ] mod support
- [ ] add local images in score displat website for mods and rankings (maybe rankedtype)
- [x] rename some Colums to enable "better" searching
- [x] example for costume webpages
- [ ] api documentaion / renaming
- [ ] api endpoint for custome querys
- [ ] pp calc for potenial scores on scorepage
- [ ] change chart value from seconds to minutes
- [ ] charts on score page (performance over time with filters)
- [x] replace modtext with mod icons in recent score
- [ ] importer for [circle-tracker](https://github.com/FunOrange/circle-tracker)
- [ ] add more stuff to the beatmap helper (images maybe) if someone asks or i need it i guess?
- [ ] score comparision of friends (or leaderboards)
- [ ] short hand overview of recent activties (played xh this week, so many scores etc...)
- [ ] rework beatmap info fetching aswell as supporting non submitted stuff, parse .osu file to get data
- [ ] player table with changes and maybe merge bancho and wasted time table
- [ ] properly take time (account for pauses etc)
- [ ] rework database
- [ ] use proper front end framework for maybe faster score loading or sometning
- [ ] replace modtext with mod icons in recent score
- [ ] api documentaion / renaming
- [ ] api endpoint for custome querys
</details>

