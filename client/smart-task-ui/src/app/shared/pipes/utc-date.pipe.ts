import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'utcDate',
  standalone: true,
})
export class UtcDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined): Date | null {
    if (!value) return null;

    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? null : value;
    }

    const normalizedValue = value.trim();
    if (!normalizedValue) return null;

    // API DateTime values may be serialized without a timezone designator.
    // Those values are UTC instants, so make the assumption explicit before
    // letting Angular's date pipe render them in the browser's local timezone.
    const hasTimezone = /(Z|[+-]\d{2}:?\d{2})$/i.test(normalizedValue);
    const date = new Date(hasTimezone ? normalizedValue : `${normalizedValue}Z`);

    return Number.isNaN(date.getTime()) ? null : date;
  }
}
