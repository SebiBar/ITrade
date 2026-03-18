# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source files
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy built files from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration as a template for envsubst
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# Ensure that only BACKEND_URL is substituted in the template
ENV NGINX_ENVSUBST_FILTER=BACKEND_URL

# Expose port 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
