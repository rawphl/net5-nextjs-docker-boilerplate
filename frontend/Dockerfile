FROM node:alpine

# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Install app dependencies
COPY . /usr/src/app
RUN npm install
# Build the application
RUN npm run build 
RUN apk update && apk add bash
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh

