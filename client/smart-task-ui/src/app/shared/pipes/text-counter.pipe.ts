import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'textCounter',
  standalone: true,
})
export class TextCounterPipe implements PipeTransform {
  transform(value: string | null | undefined, maxLength: number): string {
    const normalizedValue = value ?? '';
    const trimmedValue = normalizedValue.trim();
    const wordCount = trimmedValue ? trimmedValue.split(/\s+/).length : 0;

    return `${wordCount} words | ${normalizedValue.length}/${maxLength}`;
  }
}
