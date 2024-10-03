import React, { useState } from 'react';
import { addLanguage } from '../../../services/LanguageService';
import { toast } from 'react-toastify';

const AddLanguageContainer: React.FC = () => {
    const [languageName, setLanguageName] = useState("");

    const handleAddLanguage = async () => {
        if (!languageName) {
            toast.error("Language name cannot be empty.");
            return;
        }

        try {
            const result = await addLanguage(languageName);
            if (result.success)
                toast.success(result.message);
            else
                toast.error(result.message);
        } catch (error) {
            if (error instanceof Error)
                toast.error(error.message);
            else
                toast.error("An unknown error occurred.");
        }
    };

    return (
        <div>
            <input
                type="text"
                value={languageName}
                onChange={(e) => setLanguageName(e.target.value)}
            />
            <button onClick={handleAddLanguage}>Add Language</button>
        </div>
    );
};

export default AddLanguageContainer;