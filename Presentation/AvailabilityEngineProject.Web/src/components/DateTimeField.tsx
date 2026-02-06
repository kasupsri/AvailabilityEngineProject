import { useMemo, useCallback, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { cn } from '@/lib/utils';

const HOURS = Array.from({ length: 24 }, (_, i) => i);
const MINUTE_OPTIONS = [0, 15, 30, 45];

function snapMinute(m: number): number {
  const snapped = Math.round(m / 15) * 15;
  if (snapped <= 0) return 0;
  if (snapped >= 45) return 45;
  return snapped;
}

function isoToLocal(iso: string): { dateStr: string; hour: number; minute: number } {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) {
    const now = new Date();
    return {
      dateStr:
        now.getFullYear() +
        '-' +
        String(now.getMonth() + 1).padStart(2, '0') +
        '-' +
        String(now.getDate()).padStart(2, '0'),
      hour: now.getHours(),
      minute: snapMinute(now.getMinutes()),
    };
  }
  return {
    dateStr:
      d.getFullYear() +
      '-' +
      String(d.getMonth() + 1).padStart(2, '0') +
      '-' +
      String(d.getDate()).padStart(2, '0'),
    hour: d.getHours(),
    minute: snapMinute(d.getMinutes()),
  };
}

function localToIso(dateStr: string, hour: number, minute: number): string {
  const [y, m, d] = dateStr.split('-').map(Number);
  if (!y || !m || !d) return new Date().toISOString();
  const date = new Date(y, m - 1, d, hour, minute, 0, 0);
  return date.toISOString();
}

export interface DateTimeFieldProps {
  value: string;
  onChange: (iso: string) => void;
  id?: string;
  label?: string;
  className?: string;
  disabled?: boolean;
}

const triggerClass =
  'h-10 min-h-[44px] w-full min-w-0 rounded-md border border-input bg-background px-3 py-2 text-sm tabular-nums transition-colors duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50';

export function DateTimeField({ value, onChange, id: idProp, label, className, disabled }: DateTimeFieldProps) {
  const id = useMemo(() => idProp ?? `datetime-${Math.random().toString(36).slice(2)}`, [idProp]);
  const { dateStr, hour, minute } = useMemo(() => isoToLocal(value), [value]);

  useEffect(() => {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return;
    const rawMinute = d.getMinutes();
    if (!MINUTE_OPTIONS.includes(rawMinute)) onChange(localToIso(dateStr, hour, snapMinute(rawMinute)));
  }, [value, dateStr, hour, onChange]);

  const emit = useCallback(
    (next: { dateStr?: string; hour?: number; minute?: number }) => {
      const d = next.dateStr ?? dateStr;
      const h = next.hour ?? hour;
      const m = next.minute ?? minute;
      onChange(localToIso(d, h, m));
    },
    [dateStr, hour, minute, onChange]
  );

  return (
    <div className={cn('flex flex-col gap-2', className)} role="group" aria-labelledby={label ? `${id}-label` : undefined}>
      {label && (
        <Label id={`${id}-label`} htmlFor={`${id}-date`} className="text-sm font-medium leading-tight">
          {label}
        </Label>
      )}
      <div className="flex flex-wrap items-stretch gap-2">
        <div className="flex flex-col gap-1 min-w-[140px] flex-1">
          <Input
            id={`${id}-date`}
            type="date"
            value={dateStr}
            onChange={(e) => emit({ dateStr: e.target.value })}
            disabled={disabled}
            className="h-10 min-h-[44px] transition-colors duration-200"
            aria-label={label ? `${label}, date` : 'Date'}
          />
          {!label && (
            <span className="text-xs text-muted-foreground" aria-hidden>
              Date
            </span>
          )}
        </div>
        <div className="flex items-center gap-1 shrink-0">
          <Select value={String(hour)} onValueChange={(v) => emit({ hour: Number(v) })} disabled={disabled}>
            <SelectTrigger
              id={`${id}-hour`}
              className={cn(triggerClass, 'w-[4.5rem]')}
              aria-label={label ? `${label}, hour` : 'Hour'}
            >
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {HOURS.map((h) => (
                <SelectItem key={h} value={String(h)} className="tabular-nums">
                  {String(h).padStart(2, '0')}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <span className="text-muted-foreground text-sm font-medium px-0.5 self-center" aria-hidden>
            :
          </span>
          <Select value={String(minute)} onValueChange={(v) => emit({ minute: Number(v) })} disabled={disabled}>
            <SelectTrigger
              id={`${id}-minute`}
              className={cn(triggerClass, 'w-[4.5rem]')}
              aria-label={label ? `${label}, minute` : 'Minute'}
            >
              <SelectValue>
                {String(minute).padStart(2, '0')}
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              {MINUTE_OPTIONS.map((m) => (
                <SelectItem key={m} value={String(m)} className="tabular-nums">
                  {String(m).padStart(2, '0')}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>
  );
}
