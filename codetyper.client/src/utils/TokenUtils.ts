import { jwtDecode } from "jwt-decode";

interface TokenPayload {
    username: string;
    role: string;
    userId: string;
    exp: number;
}

export const extractUserInfo = (token: string) => {
    try {
        const decoded = jwtDecode<TokenPayload>(token);

        return {
            username: decoded.username,
            role: decoded.role,
            userId: decoded.userId
        };
    } catch (error) {
        console.error('Failed to decode token:', error);
        return null;
    }
};

export const isTokenExpired = (token: string): boolean => {
    try {
        const decoded = jwtDecode<TokenPayload>(token);
        const currentTime = Math.floor(Date.now() / 1000); // current time in seconds

        return decoded.exp < currentTime; // check if token is expired
    } catch (error) {
        console.error("Failed to decode token:", error);
        return true; // expired if decoding fails
    }
};