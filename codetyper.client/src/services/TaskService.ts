import { API_BASE_URL } from "../configurations/config";
import { CodingTask } from "../models/CodingTask";
import User from "../models/User";
import { ApiResponse, fetchWithToken } from "./AuthService";

interface ApiGetResponse {
    success: boolean;
    message?: string;

    tasks?: CodingTask[];
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

export interface RandomTaskResponse {
    Task: CodingTask;
    Creator: User;
    Count: number;
}

export const getRandomTask = async (): Promise<RandomTaskResponse | null> => {
    try {
        const response = await fetchWithToken(`/api/tasks/randomRequest`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data: RandomTaskResponse = await response.json();
        return data;
    } catch (error) {
        console.error("Error fetching the random task:", error);
        return null;
    }
};


export const addTask = async (taskData: CodingTask): Promise<ApiResponse> => {
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

export const getAllShownTasks = async (page: number = 1, pageSize: number = 15): Promise<ApiGetResponse> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/tasks/shown?page=${page}&pageSize=${pageSize}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const { tasks, currentPage, pageSize: responsePageSize, totalPages } = await response.json();

        return {
            success: true,
            tasks: tasks,
            currentPage: currentPage,
            pageSize: responsePageSize,
            totalPages: totalPages
        };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message, currentPage: page, pageSize: pageSize, totalPages: 0 };
        } else {
            return { success: false, message: "An unknown error occurred.", currentPage: page, pageSize: pageSize, totalPages: 0 };
        }
    }
};