import { useEffect, useState } from "react";
import { getAllLanguages } from "../../../services/LanguageService";
import Language from "../../../models/Language";
import { toast } from "react-toastify";

interface SelectLanguageProps {
    onLanguageChange: (language: Language) => void;
}

const SelectLanguage: React.FC<SelectLanguageProps> = ({ onLanguageChange }) => {
    const [languages, setLanguages] = useState<Language[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchLanguages = async () => {
            setLoading(true);
            const result = await getAllLanguages();

            if (result.success && result.data) {
                setLanguages(result.data);
            } else {
                toast.error(result.message);
            }
            setLoading(false);
        };
        fetchLanguages();
    }, []);

    const handleChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        const selectedLanguage = languages.find(lang => lang.name === event.target.value);
        if (selectedLanguage)
            onLanguageChange(selectedLanguage);
    }

    return (
        <div>
            {loading ? (
                <p>Loading languages...</p>
            ) : (
                    <select onChange={handleChange}>
                    <option value="">Select a language</option>
                    {languages.map((lang, index) => (
                        <option key={index} value={lang.name}>
                        {lang.name}
                    </option>
                ))}
                </select>
            )}
        </div>
    );
}

export default SelectLanguage;