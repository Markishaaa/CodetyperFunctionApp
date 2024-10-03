import { API_BASE_URL } from "../configurations/config";
import { ApiResponse, fetchWithToken } from "./AuthService";

export const addLanguage = async (name: string): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`${API_BASE_URL}/api/addLanguage`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ name }),
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