import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from 'axios';

// Token management
class TokenManager {
    private static JWT_KEY = 'jwt_token';
    private static REFRESH_KEY = 'refresh_token';

    static getJwtToken(): string | null {
        return localStorage.getItem(this.JWT_KEY);
    }

    static getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH_KEY);
    }

    static setTokens(jwt: string, refresh: string): void {
        localStorage.setItem(this.JWT_KEY, jwt);
        localStorage.setItem(this.REFRESH_KEY, refresh);
    }

    static clearTokens(): void {
        localStorage.removeItem(this.JWT_KEY);
        localStorage.removeItem(this.REFRESH_KEY);
    }
}

// Create axios instance
const apiClient: AxiosInstance = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor to add JWT token
apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = TokenManager.getJwtToken();
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error: AxiosError) => {
        return Promise.reject(error);
    }
);

// Response interceptor for handling token refresh
let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value?: unknown) => void;
    reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });

    failedQueue = [];
};

apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & {
            _retry?: boolean;
        };

        // If error is 401 and we haven't tried to refresh yet
        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                // If already refreshing, queue this request
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then(() => {
                        return apiClient(originalRequest);
                    })
                    .catch((err) => {
                        return Promise.reject(err);
                    });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            const refreshToken = TokenManager.getRefreshToken();
            if (!refreshToken) {
                // No refresh token available, redirect to login
                TokenManager.clearTokens();
                window.location.href = '/login';
                return Promise.reject(error);
            }

            try {
                // Attempt to refresh the token
                const response = await axios.post(
                    `${apiClient.defaults.baseURL}/auth/refresh-tokens`,
                    null,
                    {
                        params: { refreshToken },
                    }
                );

                const { jwt, refresh } = response.data;
                TokenManager.setTokens(jwt, refresh);

                // Update the original request with new token
                if (originalRequest.headers) {
                    originalRequest.headers.Authorization = `Bearer ${jwt}`;
                }

                processQueue(null, jwt);
                isRefreshing = false;

                return apiClient(originalRequest);
            } catch (refreshError) {
                processQueue(refreshError as Error, null);
                isRefreshing = false;
                TokenManager.clearTokens();
                window.location.href = '/login';
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export { apiClient, TokenManager };
export default apiClient;
