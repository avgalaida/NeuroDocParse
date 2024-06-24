FROM node:16-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install 

# RUN npm install --save graphql graphql-tag @apollo/client @vue/apollo-option apollo-upload-client

COPY . .

EXPOSE 5137

CMD ["npm", "run", "serve"]