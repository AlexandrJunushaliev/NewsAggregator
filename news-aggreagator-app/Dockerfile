FROM node:latest AS builder

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .

RUN npm run build

FROM nginx:latest
COPY ./site.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist /app

CMD [ "nginx", "-g", "daemon off;" ]
