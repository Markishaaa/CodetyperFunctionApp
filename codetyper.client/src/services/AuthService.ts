import { API_BASE_URL } from "../configurations/config";
import { extractUserInfo, isTokenExpired } from "../utils/TokenUtils";

interface UserData {
    username: string;
    password: string;
    email?: string;
}

interface LoginData {
    username: string;
    password: string;
}

interface ApiResponse {
    success: boolean;
    message: string;
    token?: string;
}

const getAuthToken = (): string | null => {
    const token = sessionStorage.getItem('token');

    if (token && isTokenExpired(token)) {
        removeUserData();

        window.location.href = '/login';
        return null;
    }

    return token;
};

/*
    // example usage

    const response = await fetchWithToken('/GetUserData', {
            method: 'GET',
        });

        if (!response.ok) {
            throw new Error('Failed to fetch user data');
        }

        return await response.json();
*/
export const fetchWithToken = async (endpoint: string, options: RequestInit = {}) => {
    const token = getAuthToken();
    if (!token) {
        throw new Error('Your session has timed out. Please log in again.');
    }

    options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`,
    };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
    return response;
};

export const registerUser = async (userData: UserData): Promise<ApiResponse> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/RegisterUser`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(userData),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data = await response.text();
        return { success: true, message: data };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};

export const loginUser = async (loginData: LoginData): Promise<ApiResponse> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/LoginUser`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(loginData),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data = await response.text();
        const token = response.headers.get("Authorization")?.split(" ")[1];

        if (token) {
            sessionStorage.setItem("token", token);
            storeUserData(token);
        }

        return { success: true, message: data, token };
    } catch (error: unknown) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};

export const logoutUser = (): void => {
    removeUserData();

    window.location.href = '/auth';
};

const storeUserData = (token: string) => {
    const userInfo = extractUserInfo(token);

    if (userInfo) {
        console.log("username: " + userInfo.username);
        sessionStorage.setItem('username', userInfo.username);
        sessionStorage.setItem('role', userInfo.role);
    }
}

const removeUserData = () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('username');
    sessionStorage.removeItem('role');
}

export default UserData;