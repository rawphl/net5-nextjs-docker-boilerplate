FROM node:10-alpine

# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Install app dependencies
COPY . /usr/src/app
RUN npm install
# Build the application
RUN npm run build

COPY init_container.sh /opt/
# CMD [ "npm", "start" ]
ENTRYPOINT [ "/opt/init_container.sh" ]

