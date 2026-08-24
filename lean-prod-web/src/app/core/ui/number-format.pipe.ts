import { Pipe, PipeTransform } from '@angular/core';

function formatFixed(value: number | string | null | undefined, fractionDigits: number): string {
  if (value === null || value === undefined || value === '') return '';
  const numericValue = Number(value);
  if (!Number.isFinite(numericValue)) return '';

  const sign = numericValue < 0 ? '-' : '';
  const [integerPart, decimalPart] = Math.abs(numericValue).toFixed(fractionDigits).split('.');
  const groupedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
  return `${sign}${groupedInteger}.${decimalPart}`;
}

@Pipe({ name: 'moneyFormat', standalone: true })
export class MoneyFormatPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    return formatFixed(value, 2);
  }
}

@Pipe({ name: 'quantityFormat', standalone: true })
export class QuantityFormatPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    return formatFixed(value, 3);
  }
}
