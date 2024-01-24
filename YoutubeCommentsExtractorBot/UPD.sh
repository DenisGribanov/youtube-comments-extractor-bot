#!/bin/bash

cd ~/gitlab/youtube-comments-extractor-bot/

git stash
git stash drop

PULL_RESULT=$(git pull)

Already="Already up to date."

cd ~

docker container stop comments_extractor_bot_v2

docker container rm comments_extractor_bot_v2

docker image rm comments_extractor_bot_v2:v.0.1

cp -f ~/gitlab/youtube-comments-extractor-bot/YoutubeCommentsExtractorBot/BotApi/Dockerfile ~/gitlab/youtube-comments-extractor-bot/YoutubeCommentsExtractorBot/Dockerfile

docker build -t comments_extractor_bot_v2:v.0.1 gitlab/youtube-comments-extractor-bot/YoutubeCommentsExtractorBot

docker run -v ~/commentsextractor_files:/app/files --name comments_extractor_bot_v2 -d -p 8589:8080 comments_extractor_bot_v2:v.0.1
