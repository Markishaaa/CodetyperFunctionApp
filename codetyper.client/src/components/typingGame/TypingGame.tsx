/* eslint-disable @typescript-eslint/no-unused-vars */
import React, { useEffect, useState } from 'react';
import { toast } from 'react-toastify';
import CodeMirrorEditor from '../CodeMirrorEditor';
import './typingGame.css';
import { getRandomSnippet } from '../../services/SnippetService';
import { CodeSnippet } from '../../models/CodeSnippet';

const staticSnippet = `console.log("Hello, World!");
let x = 10;
if (x > 5) { console.log("X is greater than 5"); }`;

const TypingGame: React.FC<{ resetTrigger: number }> = ({ resetTrigger }) => {
    const [snippet, setSnippet] = useState<CodeSnippet>();

    const [userInput, setUserInput] = useState<string>('');
    const [currentSnippet, setCurrentSnippet] = useState<string>(staticSnippet);
    const [timerStarted, setTimerStarted] = useState<boolean>(false);

    const [seconds, setSeconds] = useState<number>(0);
    const [milliseconds, setMilliseconds] = useState<number>(0);
    const [timeDisplay, setTimeDisplay] = useState<string>('00:00:000');

    useEffect(() => {
        // reset the game state when resetTrigger changes
        setUserInput('');
        setTimerStarted(false);
        setSeconds(0);
        setMilliseconds(0);
        setTimeDisplay('00:00:000');
    }, [resetTrigger]);

    useEffect(() => {
        const fetchSnippet = async () => {
            const fetchedSnippet = await getRandomSnippet();
            if (fetchedSnippet == null)
                return;

            setSnippet(fetchedSnippet);
            setCurrentSnippet(fetchedSnippet.content);
        };

        fetchSnippet();
    }, [resetTrigger]);

    // Timer logic
    useEffect(() => {
        let timer: NodeJS.Timeout | null = null;

        if (timerStarted) {
            timer = setInterval(() => {
                setMilliseconds(prevMilliseconds => {
                    if (prevMilliseconds >= 999) {
                        setSeconds(prevSeconds => prevSeconds + 1); // increment seconds when milliseconds hit 1000
                        return 0; // reset milliseconds after hitting 1000
                    }
                    return prevMilliseconds + 10; // increment milliseconds by 10
                });
            }, 10); // update every 10ms
        }

        return () => {
            if (timer) {
                clearInterval(timer);
            }
        };
    }, [timerStarted]);

    useEffect(() => {
        const minutes = Math.floor(seconds / 60);
        const secs = seconds % 60;
        const formattedMilliseconds = String(milliseconds).padStart(3, '0');
        const formattedTime = `${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}:${formattedMilliseconds}`;
        setTimeDisplay(formattedTime);
    }, [seconds, milliseconds]);

    const calculateWpm = () => {
        const charactersTyped = userInput.length;
        const minutes = seconds / 60;
        const wpmCalc = charactersTyped > 0 && minutes > 0 ? (charactersTyped / 5) / minutes : 0;
        return parseFloat(wpmCalc.toFixed(2));
    };

    const handleKeydown = (_editor: never, event: KeyboardEvent) => {
        if (event.ctrlKey && ['s', 'v', 'z', 'c'].includes(event.key.toLowerCase())) {
            event.preventDefault();
        }

        if (event.key === 'Tab' || event.key === 'Enter') {
            return;
        }

        if (!timerStarted && event.key) {
            setTimerStarted(true);
        }
    };

    const handleBeforePaste = (_editor: never, event: ClipboardEvent) => {
        event.preventDefault(); 
    };

    const createScore = (wpm: number) => {
        toast.info(`WPM: ${wpm}`);
    };

    const normalizeCode = (code: string) => {
        return code
            .replace(/\s+/g, ' ')  // replace multiple spaces/tabs/newlines with a single space
            .trim();               
    };

    const isCodeMatching = () => {
        return normalizeCode(userInput) === normalizeCode(currentSnippet);
    };

    // stop timer and show score when code matches
    useEffect(() => {
        if (timerStarted && isCodeMatching()) {
            setTimerStarted(false);  
            const wpmResult = calculateWpm(); 
            createScore(wpmResult); 
        }
    }, [userInput, currentSnippet, timerStarted]);

    return (
        <div className="container-editor">
            <div className="timer-display">
                <h2 className="text-center">{timeDisplay}</h2>
            </div>

            <div className="wrapper-editor">
                <div className="row">
                    <div className="col">
                        <CodeMirrorEditor
                            value={currentSnippet}
                            onChange={setCurrentSnippet}
                            language={snippet ? snippet.languageName.toLowerCase() : ""}
                            readOnly='nocursor'
                            handleKeyDown={handleKeydown}
                            handleOnPaste={handleBeforePaste}
                        />
                    </div>
                    <div className="col editor">
                        <CodeMirrorEditor
                            value={userInput}
                            onChange={setUserInput}
                            language={snippet ? snippet.languageName.toLowerCase() : ""}
                            readOnly={false}
                            handleKeyDown={handleKeydown}
                            handleOnPaste={handleBeforePaste}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TypingGame;
