import { API_BASE_URL } from "../configurations/config";
import { ApiResponse, fetchWithToken } from "./AuthService";

export interface TaskData {
    name: string;
    description: string;
    creatorId: string;
}

export const addTask = async (taskData: TaskData): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken('/api/tasks/add', {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(taskData),
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