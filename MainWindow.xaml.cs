using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PAC4_Calculadora
{
    public partial class MainWindow : Window
    {
        private string entradaActual = "";
        private bool reiniciarPantalla = false;
        private bool errorMostrado = false;
        private bool puntoDecimalAgregado = false;

        public MainWindow()
        {
            InitializeComponent();
        }

     
        private void Numero_Click(object sender, RoutedEventArgs e)
        {
            if (errorMostrado)
            {
                LimpiarTodo();
                errorMostrado = false;
            }

            if (reiniciarPantalla)
            {
                Pantalla.Text = "";
                entradaActual = "";
                reiniciarPantalla = false;
                puntoDecimalAgregado = false;
            }

            var boton = (Button)sender;
            string numero = boton.Content.ToString();

            if (numero == "0" && (Pantalla.Text == "0" || string.IsNullOrEmpty(Pantalla.Text)))
            {
                return;
            }

            entradaActual += numero;
            Pantalla.Text += numero;
        }

        private void PuntDecimal_Click(object sender, RoutedEventArgs e)
        {
            if (errorMostrado)
            {
                LimpiarTodo();
                errorMostrado = false;
            }

            if (reiniciarPantalla)
            {
                Pantalla.Text = "";
                entradaActual = "0";
                reiniciarPantalla = false;
                puntoDecimalAgregado = false;
            }

            if (!puntoDecimalAgregado)
            {
                if (string.IsNullOrEmpty(entradaActual) || EsOperador(entradaActual[entradaActual.Length - 1]))
                {
                    entradaActual += "0";
                    Pantalla.Text += "0";
                }

                entradaActual += ".";
                Pantalla.Text += ".";
                puntoDecimalAgregado = true;
            }
        }

        
        private void Operador_Click(object sender, RoutedEventArgs e)
        {
            if (errorMostrado)
            {
                LimpiarTodo();
                errorMostrado = false;
                return;
            }

            var boton = (Button)sender;
            string nuevoOperador = boton.Content.ToString();

            if (string.IsNullOrEmpty(entradaActual) && nuevoOperador != "-")
            {
                return;
            }

            if (Pantalla.Text.Length > 0 && EsOperador(Pantalla.Text[^1]))
            {
                Pantalla.Text = Pantalla.Text[..^1];
                entradaActual = entradaActual[..^1];
            }

            entradaActual += nuevoOperador;
            Pantalla.Text += nuevoOperador;
            reiniciarPantalla = false;
            puntoDecimalAgregado = false;
        }

       
        private void Igual_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(entradaActual) || errorMostrado)
            {
                return;
            }

            try
            {
                string expresionLimpia = entradaActual.Replace(",", ".");

                if (expresionLimpia.Length > 0 && EsOperador(expresionLimpia[^1]))
                {
                    throw new SyntaxErrorException("La expresión termina en un operador.");
                }

                if (TieneOperadoresConsecutivos(expresionLimpia))
                {
                    throw new SyntaxErrorException("No se permiten operadores consecutivos.");
                }

                if (Regex.IsMatch(expresionLimpia, @"([^\.0-9]|^)0([^\.0-9]|$)"))
                {
                    throw new DivideByZeroException();
                }

                double resultado = EvaluarExpresion(expresionLimpia);

                Pantalla.Text = FormatearResultado(resultado);
                entradaActual = Pantalla.Text.Replace(",", ".");
                reiniciarPantalla = true;
                puntoDecimalAgregado = Pantalla.Text.Contains(",");
            }
            catch (DivideByZeroException)
            {
                MostrarError();
            }
            catch (SyntaxErrorException)
            {
                MostrarError();
            }
            catch (Exception ex) when (ex is EvaluateException || ex is OverflowException)
            {
                MostrarError();
            }
        }

  
        private bool TieneOperadoresConsecutivos(string expresion)
        {
            for (int i = 1; i < expresion.Length; i++)
            {
                if (EsOperador(expresion[i]) && EsOperador(expresion[i - 1]))
                {
                    return true;
                }
            }
            return false;
        }

        
        private double EvaluarExpresion(string expresion)
        {
            var dataTable = new DataTable();
            var resultado = dataTable.Compute(expresion, null);

            if (resultado.ToString() == "∞" || double.IsInfinity(Convert.ToDouble(resultado)))
            {
                throw new DivideByZeroException();
            }

            return Convert.ToDouble(resultado);
        }

        private string FormatearResultado(double resultado)
        {
            if (resultado == Math.Truncate(resultado))
            {
                return resultado.ToString("0", CultureInfo.CurrentCulture);
            }
            else
            {
                string formateado = resultado.ToString("0.##########", CultureInfo.CurrentCulture);
                if (formateado.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    formateado = formateado.TrimEnd('0');
                    if (formateado.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    {
                        formateado = formateado[..^1];
                    }
                }
                return formateado;
            }
        }

        
        private void MostrarError()
        {
            Pantalla.Text = "Error";
            entradaActual = "";
            reiniciarPantalla = true;
            errorMostrado = true;
            puntoDecimalAgregado = false;
        }

      
        private void Neteja_Click(object sender, RoutedEventArgs e)
        {
            LimpiarTodo();
        }

     
        private void LimpiarTodo()
        {
            Pantalla.Text = "";
            entradaActual = "";
            reiniciarPantalla = false;
            errorMostrado = false;
            puntoDecimalAgregado = false;
        }

      
        private bool EsOperador(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/';
        }
    }
}
