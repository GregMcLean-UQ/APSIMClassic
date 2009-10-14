﻿Imports System
Imports System.Collections.Generic
Imports System.Text
Imports ModelFramework
Imports ManagerHelpers
Imports CSGeneral


Public Class Root
    Inherits Instance
    <Output()> <Param()> Private xf As Double()
    <Output()> <Param()> Private kl As Double()
    <Output()> <Param()> Private ll As Double()
    

    <Output()> Public ReadOnly Property SWSupply() As Double()
        Get
            Dim MyPaddock As New PaddockType(Root)
         Dim dlayer As Single() = MyPaddock.SoilWater.dlayer
         Dim sw_dep As Single() = MyPaddock.SoilWater.sw_dep

            Dim Supply(dlayer.Length - 1) As Double
            Dim layer As Integer
            For layer = 0 To dlayer.Length - 1
                Supply(layer) = Math.Max(0.0, kl(layer) * (sw_dep(layer) - ll(layer) * dlayer(layer)))
            Next
            Return Supply
        End Get
    End Property
    <Output()> Public Property SWUptake() As Single
        Set(ByVal value As Single)
            Dim MyPaddock As New PaddockType(Root)

            Dim Supply As Double() = SWSupply()
            Dim Fraction As Single = value / MathUtility.Sum(Supply)
            If Fraction > 1 Then Fraction = 1

            Dim layer As Integer
            Dim SWdep(Supply.Length) As Single
         SWdep = MyPaddock.SoilWater.sw_dep
            For layer = 0 To Supply.Length - 1
                SWdep(layer) = SWdep(layer) - Fraction * Supply(layer)
            Next
         MyPaddock.SoilWater.sw_dep = SWdep

        End Set
        Get
            Return 0
        End Get
    End Property

End Class
