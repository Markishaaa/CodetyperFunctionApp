import { toast } from "react-toastify";
import User from "../models/User";
import { API_BASE_URL } from "../configurations/config";

export const getUserById = async (userId: string): Promise<User | null> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            }
        });

        if (!response.ok) {
            if (response.status === 404) {
                toast.error("User not found.");
            } else {
                const errorMessage = await response.text();
                throw new Error(errorMessage);
            }
            return null;
        }

        const user: User = await response.json();
        return user;
    } catch (error) {
        console.log(error);
        toast.error("Error fetching user.");
        return null;
    }
};