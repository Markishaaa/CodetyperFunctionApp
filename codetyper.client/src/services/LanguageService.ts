import { API_BASE_URL } from "../configurations/config";
import Language from "../models/Language";
import { ApiResponse, fetchWithToken } from "./AuthService";

interface ApiGetResponse {
    success: boolean;
    message?: string;
    data: Language[] | null;
}

export const addLanguage = async (name: string): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/AddLanguage`, {
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

export const getAllLanguages = async (): Promise<ApiGetResponse> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/languages/GetAll`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data: Language[] = await response.json();
        return { success: true, data };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message, data: null };
        } else {
            return { success: false, message: "An unknown error occurred.", data: null };
        }
    }
}