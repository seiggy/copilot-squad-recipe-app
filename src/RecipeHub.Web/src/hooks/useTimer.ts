import { useCallback, useEffect, useRef, useState } from 'react';

export interface UseTimerResult {
  remainingSeconds: number;
  isRunning: boolean;
  start: () => void;
  pause: () => void;
  reset: () => void;
}

export function useTimer(totalMinutes: number | null | undefined): UseTimerResult {
  const initialSeconds = Math.max(0, Math.round((totalMinutes ?? 0) * 60));
  const [remainingSeconds, setRemainingSeconds] = useState(initialSeconds);
  const [isRunning, setIsRunning] = useState(false);
  const intervalRef = useRef<number | null>(null);

  useEffect(() => {
    setRemainingSeconds(initialSeconds);
    setIsRunning(false);
  }, [initialSeconds]);

  useEffect(() => {
    if (!isRunning) return;
    intervalRef.current = window.setInterval(() => {
      setRemainingSeconds((s) => {
        if (s <= 1) {
          setIsRunning(false);
          return 0;
        }
        return s - 1;
      });
    }, 1000);
    return () => {
      if (intervalRef.current !== null) {
        window.clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [isRunning]);

  const start = useCallback(() => {
    if (remainingSeconds > 0) setIsRunning(true);
  }, [remainingSeconds]);

  const pause = useCallback(() => setIsRunning(false), []);

  const reset = useCallback(() => {
    setIsRunning(false);
    setRemainingSeconds(initialSeconds);
  }, [initialSeconds]);

  return { remainingSeconds, isRunning, start, pause, reset };
}
