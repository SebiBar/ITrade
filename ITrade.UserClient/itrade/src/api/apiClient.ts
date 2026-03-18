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

const toNonEmptyString = (value: unknown): string | null => {
    if (typeof value !== 'string') {
        return null;
    }

    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
};

const extractServerMessage = (data: unknown): string | null => {
    const asPlainString = toNonEmptyString(data);
    if (asPlainString) {
        return asPlainString;
    }

    if (!data || typeof data !== 'object') {
        return null;
    }

    const payload = data as Record<string, unknown>;
    const detail = toNonEmptyString(payload.detail);
    if (detail) {
        return detail;
    }

    const message = toNonEmptyString(payload.message);
    if (message) {
        return message;
    }

    const title = toNonEmptyString(payload.title);
    if (title) {
        return title;
    }

    const errors = payload.errors;
    if (errors && typeof errors === 'object') {
        for (const value of Object.values(errors as Record<string, unknown>)) {
            if (Array.isArray(value)) {
                for (const item of value) {
                    const errorMessage = toNonEmptyString(item);
                    if (errorMessage) {
                        return errorMessage;
                    }
                }
            }

            const errorMessage = toNonEmptyString(value);
            if (errorMessage) {
                return errorMessage;
            }
        }
    }

    return null;
};

const toDisplayError = (error: AxiosError): Error => {
    const statusCode = error.response?.status;
    const serverMessage = extractServerMessage(error.response?.data);

    if (statusCode !== undefined && serverMessage) {
        return new Error(`HTTP ${statusCode}: ${serverMessage}`);
    }

    if (statusCode !== undefined) {
        return new Error(`HTTP ${statusCode}: ${error.message}`);
    }

    if (serverMessage) {
        return new Error(serverMessage);
    }

    return new Error(error.message || 'Request failed.');
};

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

const processQueue = (error: Error | null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve();
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
                return Promise.reject(toDisplayError(error));
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

                processQueue(null);
                isRefreshing = false;

                return apiClient(originalRequest);
            } catch (refreshError) {
                const queueError = axios.isAxiosError(refreshError)
                    ? toDisplayError(refreshError)
                    : refreshError instanceof Error
                        ? refreshError
                        : new Error('Failed to refresh token.');

                processQueue(queueError);
                isRefreshing = false;
                TokenManager.clearTokens();
                window.location.href = '/login';
                return Promise.reject(queueError);
            }
        }

        return Promise.reject(toDisplayError(error));
    }
);

export { apiClient, TokenManager };
export default apiClient;
