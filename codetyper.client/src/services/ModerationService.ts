import { ApiResponse, fetchWithToken } from "./AuthService";

const staffId = sessionStorage.getItem("userId");

export const acceptTaskRequest = async (taskId: number): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/tasks/acceptRequest/${taskId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const message = await response.text();
        return { success: true, message };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};

export const acceptSnippetRequest = async (snippetId: number): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/snippets/acceptRequest/${snippetId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const message = await response.text();
        return { success: true, message };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};

export const denyTaskRequest = async (taskId: number, reason: string): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/tasks/denyRequest/${taskId}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                reason: reason,
                staffId: staffId
            }),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const message = await response.text();
        return { success: true, message };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};

export const denySnippetRequest = async (snippetId: number, reason: string): Promise<ApiResponse> => {
    try {
        const response = await fetchWithToken(`/api/snippets/denyRequest/${snippetId}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                reason: reason,
                staffId: staffId
            }),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        const message = await response.text();
        return { success: true, message };
    } catch (error) {
        if (error instanceof Error) {
            return { success: false, message: error.message };
        } else {
            return { success: false, message: "An unknown error occurred." };
        }
    }
};