import React, { useState } from 'react';
import { addLanguage } from '../../../services/LanguageService';

const AddLanguageContainer: React.FC = () => {
    const [languageName, setLanguageName] = useState("");

    const handleAddLanguage = async () => {
        console.log('Language Name:', languageName);
        try {
            await addLanguage(languageName);
            console.log("successfully added");
        } catch (error) {
            if (error instanceof Error)
                console.error(error.message);
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