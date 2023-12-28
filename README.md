# TelegramReminderBot
Unfinished personal project

Docker stuff:<br>
docker stop reminderbotservice<br>
docker rm  reminderbotservice<br>
docker load -i /home/pi/TelegramBotService/Images/reminderbotservice.tar<br>
docker run -d --name=reminderbotservice -v reminderbotservice_data:/App/Data reminderbotservice:1.1<br>
