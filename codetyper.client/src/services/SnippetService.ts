import { toast } from "react-toastify";
import { ApiResponse, fetchWithToken } from "./AuthService";
import { CodeSnippet } from "../models/CodeSnippet";
import { API_BASE_URL } from "../configurations/config";

export interface CodeSnippetData {
    content: string;
    languageName: string;
    taskId: number;
    creatorId: string;
}

export interface CodeSnippetDetails {
    id: number;
    content: string;
    languageName: string;
    creatorId: string;
}

export interface PaginatedSnippets {
    snippets: CodeSnippetDetails[];
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

export interface SnippetTaskDetails {
    name: string;
    description: string;
}

export interface SnippetCreator {
    userId: string;
    username: string;
    roleName: string
}

export interface RandomSnippetResponse {
    success: boolean;
    message?: string;

    snippet?: CodeSnippet;
    task?: SnippetTaskDetails;
    creator?: SnippetCreator;
}

export const addCodeSnippet = async (snippetData: CodeSnippetData): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/snippets/add`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(snippetData),
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

export const getRandomSnippet = async (): Promise<CodeSnippet | null> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/snippets/random`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            if (response.status === 404) {
                console.error("No snippets available.");
            } else {
                console.error("Failed to fetch snippet:", await response.text());
            }
            return null;
        }

        const snippet: CodeSnippet = await response.json();
        return snippet;
    } catch (error) {
        console.log(error);
        toast.error("Error fetching snippet.");
        return null;
    }
};

export const getSnippetsByTask = async (taskId: number, page: number = 1, pageSize: number = 10, languageName: string | null = null):
    Promise<PaginatedSnippets | null> => {
    try {
        const queryParams = new URLSearchParams({
            taskId: taskId.toString(),
            page: page.toString(),
            pageSize: pageSize.toString(),
            ...(languageName ? { languageName } : {})
        });

        const response = await fetch(`${API_BASE_URL}/api/snippets/shown?${queryParams}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data: PaginatedSnippets = await response.json();
        return data;
    } catch (error) {
        console.error("Error fetching snippets:", error);
        return null;
    }
};

export const getRandomSnippetRequest = async (): Promise<RandomSnippetResponse> => {
    try {
        const response = await fetchWithToken(`/api/snippets/randomRequest`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const data = await response.json();
        return {
            success: true, creator: data.creator, message: data.message, snippet: data.snippet, task: data.task };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};